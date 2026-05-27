using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class BossChaseController : MonoBehaviour
{
    public enum BossState { Running, Combat, Phase2_LightningHell, Dead }

    [Header("State Info")]
    public BossState currentState = BossState.Running;
    public float phase2Threshold = 0.5f;
    private bool _hasTriggeredPhase2 = false;

    [Header("Pacing & Start Logic")]
    public float startChaseThreshold = 3.0f; 
    public float maxVerticalLead = 12.0f;    
    private bool _hasStartedChase = false;

    [Header("Pathfinding")]
    public Transform[] escapeWaypoints;
    
    [Header("Dynamic Waypoints")]
    public bool useDynamicWaypoints = true;
    public float waypointUpdateInterval = 1.5f;
    public float waypointDetectionRange = 50f;
    
    [Header("Movement Settings")]
    public float runSpeed = 6f;
    public float waypointReachedDist = 1.0f;
    public float landedDelay = 0.5f;
    
    [Header("Edge Detection (Smart Jump & Patrol)")]
    public float edgeLookAhead = 1.0f; 
    public float edgeLookDown = 1.5f;  
    
    [Header("Physics")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.4f;

    [Header("Combat Settings")]
    public float combatDuration = 10f;
    private float _combatTimer = 0f;
    private bool _isWaitingForPlayerOnPlatform = false;

    [Header("Attacks")]
    public Transform meleeAttackPoint;
    public float meleeRadius = 2.0f;
    public int meleeDamage = 1;
    public float meleeCooldown = 2f;
    
    public Transform firePoint;
    public GameObject bulletPrefabRed;
    public GameObject bulletPrefabPurple;
    public float rangedCooldown = 3f;

    [Header("Phase 2: Lightning")]
    public GameObject lightningStrikePrefab;    
    public float lightningSpawnRate = 0.4f; 
    public float lightningSpawnDistanceX = 8f; 
    public float phase2MoveSpeed = 3f;     
    public float phase2ShootInterval = 4f;
    public int phase2BurstCount = 3;
    public float phase2BurstDelay = 0.3f;

    [Header("Phase 2: Arena & Drama")]
    public GameObject arenaFloorPrefab;
    public bool destroyPlatformsOnPhase2 = true;
    public float arenaSpawnHeightOffset = -5f;
    public GameObject platformDestructionVFX; 
    public AudioClip phase2TransitionSFX;
    [Tooltip("Background music to play during Phase 2")]
    public AudioClip phase2Music;     

    [Header("Visuals")]
    public Animator animator;
    public Transform visual;
    public BossHealthBar bossHealthBar;
    [Tooltip("Assign the BossBackgroundController to move background based on boss health")]
    public BossBackgroundController bossBackgroundController;

    [Header("Random Voice Lines")]
    [Tooltip("Audio clips to randomly play during combat")]
    public AudioClip[] voiceLines;
    [Tooltip("AudioSource for playing voice lines")]
    public AudioSource voiceAudioSource;
    [Tooltip("Minimum seconds between voice lines")]
    public float voiceIntervalMin = 5f;
    [Tooltip("Maximum seconds between voice lines")]
    public float voiceIntervalMax = 12f;


    [Header("Death Dialogue")]
    public DialogueData bossDeathDialogue;

    // Internal State
    private Transform _player;
    private Rigidbody2D _rb;
    private Collider2D _myCollider;
    private Collider2D[] _allMyColliders;
    private Health _health;
    private PlayerMovement _playerMove;
    
    // Logic Flags
    private int _currentWaypointIndex = 0;
    private bool _isJumping = false;       
    private bool _isWaiting = false;
    private bool _isAttacking = false;
    private float _attackStuckTimer = 0f; 
    private bool _facingRight = true;
    private bool _patrolMovingRight = true;
    
    // Phase 2 Variables
    private Vector2 _phase2HoverTarget;
    private float _phase2HoverTimer = 0f;
    private float _phase2NextShootTime = 0f;
    private bool _isDead = false;
    private float _nextAttackTime = 0f;
    private GameObject _spawnedArena;
    
    // Dynamic Waypoints
    private List<Transform> _dynamicWaypoints = new List<Transform>();
    private Dictionary<string, int> _levelPathMemory = new Dictionary<string, int>();
    private HashSet<Transform> _combatWaypoints = new HashSet<Transform>(); 
    private HashSet<Transform> _knownWaypoints = new HashSet<Transform>();  
    private int _waypointsSinceLastCombat = 0;
    private int _nextCombatGap = 3; 
    private Dictionary<Transform, string> _waypointLogMap = new Dictionary<Transform, string>();
    private int _jumpLogIndex = 0;

    private bool _phase2CombatReady = false;
    
    // Voice line timing
    private float _nextVoiceTime = 0f;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _myCollider = GetComponent<Collider2D>();
        _allMyColliders = GetComponentsInChildren<Collider2D>();
        _health = GetComponent<Health>();


        if (bossHealthBar != null)
        {
            bossHealthBar.Initialize(_health);
        }

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) _player = p.transform;

        if (_player) _playerMove = _player.GetComponent<PlayerMovement>();

        if (!groundCheck) groundCheck = transform;
        if (!meleeAttackPoint) meleeAttackPoint = transform;
        if (!firePoint) firePoint = transform;

        if (visual) {
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x);
            visual.localScale = scale;
        }
        _facingRight = true;
        _hasStartedChase = false; 

        _nextCombatGap = Random.Range(3, 6);

        EnterRunState();

        if (useDynamicWaypoints)
            StartCoroutine(UpdateDynamicWaypoints());
    }

    void Update()
    {
        if (_isDead || _player == null) return;

        // Random voice lines
        TryPlayRandomVoiceLine();

        // Update background position based on boss health
        if (bossBackgroundController != null && _health != null)
        {
            bossBackgroundController.SetBossHealthPercent(_health.GetHealthPercent());
        }

        if (_health.GetHealthPercent() <= 0 && !_isDead)
        {
            HandleDeath();
            return;
        }

        if (_isAttacking)
        {
            _attackStuckTimer += Time.deltaTime;
            if (_attackStuckTimer > 1.5f) { _isAttacking = false; _attackStuckTimer = 0f; }
        }
        else _attackStuckTimer = 0f;

        if (!_hasTriggeredPhase2 && _health.GetHealthPercent() <= phase2Threshold)
        {
            StartCoroutine(EnterPhase2LightningHell());
            return;
        }

        switch (currentState)
        {
            case BossState.Running:
                HandleRunning();
                break;
            case BossState.Combat:
                HandleCombat();
                break;
            case BossState.Phase2_LightningHell:
                HandleLightningHell();
                break;
        }
    }

    // Random Voice Lines

    void TryPlayRandomVoiceLine()
    {
        // Skip if no voice lines configured or audio source missing
        if (voiceLines == null || voiceLines.Length == 0) return;
        if (voiceAudioSource == null) return;
        
        // Skip if currently playing a voice line
        if (voiceAudioSource.isPlaying) return;
        
        // Check if it's time for next voice line
        if (Time.time >= _nextVoiceTime)
        {
            // Pick random clip
            AudioClip clip = voiceLines[Random.Range(0, voiceLines.Length)];
            voiceAudioSource.PlayOneShot(clip);
            
            // Schedule next voice line
            _nextVoiceTime = Time.time + Random.Range(voiceIntervalMin, voiceIntervalMax);
        }
    }

    // Dynamic Waypoint System

    IEnumerator UpdateDynamicWaypoints()
    {
        yield return new WaitForSeconds(0.5f);

        while (!_isDead && currentState != BossState.Phase2_LightningHell)
        {
            CollectWaypointsFromChunks();
            yield return new WaitForSeconds(waypointUpdateInterval);
        }
    }

    void CollectWaypointsFromChunks()
    {
        Transform currentTargetObj = null;
        if (escapeWaypoints != null && _currentWaypointIndex < escapeWaypoints.Length)
        {
            currentTargetObj = escapeWaypoints[_currentWaypointIndex];
        }

        GameObject[] allChunks = GameObject.FindGameObjectsWithTag("GamePlatform");
        List<Transform> newWaypoints = new List<Transform>();

        foreach (var chunkObj in allChunks)
        {
            float distanceY = chunkObj.transform.position.y - transform.position.y;
            if (distanceY < -10f || distanceY > waypointDetectionRange) continue;

            Transform waypointContainer = FindWaypointContainer(chunkObj.transform);
            if (waypointContainer == null) continue;

            string chunkID = chunkObj.GetInstanceID().ToString();

            foreach (Transform levelContainer in waypointContainer)
            {
                bool hasPathOptions = false;
                foreach(Transform child in levelContainer) {
                    if (child.childCount > 0 || child.name.Contains("Option") || child.name.Contains("Path")) {
                        hasPathOptions = true;
                        break;
                    }
                }

                Transform chosenPathContainer = levelContainer;

                if (hasPathOptions)
                {
                    string memoryKey = chunkID + "_" + levelContainer.GetSiblingIndex();
                    int chosenIndex = 0;

                    if (_levelPathMemory.ContainsKey(memoryKey))
                    {
                        chosenIndex = _levelPathMemory[memoryKey];
                        if (chosenIndex >= levelContainer.childCount) chosenIndex = 0;
                    }
                    else
                    {
                        chosenIndex = Random.Range(0, levelContainer.childCount);
                        _levelPathMemory.Add(memoryKey, chosenIndex);
                    }
                    chosenPathContainer = levelContainer.GetChild(chosenIndex);
                }

                if (chosenPathContainer.childCount > 0)
                {
                    foreach (Transform waypoint in chosenPathContainer)
                        AddWaypointToList(newWaypoints, waypoint, chunkObj, levelContainer, chosenPathContainer);
                }
                else if (chosenPathContainer != levelContainer)
                {
                    AddWaypointToList(newWaypoints, chosenPathContainer, chunkObj, levelContainer, chosenPathContainer);
                }
            }
        }

        newWaypoints = newWaypoints.OrderBy(w => w.position.y).ToList();

        AssignCombatRoles(newWaypoints);

        if (newWaypoints.Count > 0)
        {
            _dynamicWaypoints = newWaypoints;
            escapeWaypoints = _dynamicWaypoints.ToArray();
            
            if (currentTargetObj != null) 
            {
                int newIndex = _dynamicWaypoints.IndexOf(currentTargetObj);
                if (newIndex != -1) _currentWaypointIndex = newIndex;
                else FindClosestWaypointAbove();
            }
            else
            {
                FindClosestWaypointAbove();
            }
        }
    }

    void AssignCombatRoles(List<Transform> sortedWaypoints)
    {
        _knownWaypoints.RemoveWhere(wp => wp == null);
        _combatWaypoints.RemoveWhere(wp => wp == null);

        foreach (var wp in sortedWaypoints)
        {
            if (!_knownWaypoints.Contains(wp))
            {
                _knownWaypoints.Add(wp);
                _waypointsSinceLastCombat++;

                if (_waypointsSinceLastCombat >= _nextCombatGap)
                {
                    _combatWaypoints.Add(wp);
                    _waypointsSinceLastCombat = 0;
                    _nextCombatGap = Random.Range(3, 6); 
                }
            }
        }
    }

    void AddWaypointToList(List<Transform> list, Transform wp, GameObject chunk, Transform lvl, Transform path)
    {
        list.Add(wp);
        string logData = $"Chunk: {chunk.name}, Lvl: {lvl.name}, Path: {path.name}";
        if (!_waypointLogMap.ContainsKey(wp)) _waypointLogMap.Add(wp, logData);
        else _waypointLogMap[wp] = logData;
    }
    
    void FindClosestWaypointAbove()
    {
         for(int i=0; i<escapeWaypoints.Length; i++) {
            if(escapeWaypoints[i].position.y > transform.position.y) {
                _currentWaypointIndex = i;
                break;
            }
        }
    }

    Transform FindWaypointContainer(Transform parent)
    {
        Transform found = parent.Find("BossWaypoints");
        if (found != null) return found;
        foreach (Transform child in parent) {
            found = FindWaypointContainer(child);
            if (found != null) return found;
        }
        return null;
    }

    // Movement Logic

    void HandleRunning()
    {
        if (escapeWaypoints == null || escapeWaypoints.Length == 0) return;
        if (_currentWaypointIndex >= escapeWaypoints.Length) return;
        if (_isWaiting) return;

        // Chase Start Logic
        if (!_hasStartedChase)
        {
            float yDiff = transform.position.y - _player.position.y;
            if (_player.position.y >= transform.position.y || Mathf.Abs(yDiff) < startChaseThreshold)
            {
                _hasStartedChase = true;
                Debug.Log("[BOSS] Player detected. Starting Chase!");
            }
            else
            {
                StopMoving();
                FaceTarget(_player.position);
                return;
            }
        }

        // Too Far Ahead Logic
        if ((transform.position.y - _player.position.y) > maxVerticalLead)
        {
            StopMoving();
            FaceTarget(_player.position);
            return;
        }

        Transform target = escapeWaypoints[_currentWaypointIndex];
        if (target == null) { _currentWaypointIndex++; return; }

        float dist = Vector2.Distance(transform.position, target.position);
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Landing Check
        if (_isJumping && isGrounded && _rb.velocity.y <= 0.1f)
        {
            StartCoroutine(LandAndStabilize());
            return;
        }

        if (_isJumping || !isGrounded) return; // In-air physics takes over

        FaceTarget(target.position);

        // Calculations
        float heightDiff = target.position.y - transform.position.y;
        float horizDiff = Mathf.Abs(target.position.x - transform.position.x);
        
        bool targetIsAbove = heightDiff > 1.0f; 
        bool targetIsBelow = heightDiff < -2f; // Explicit command to drop down
        bool isCloseEnoughToJump = horizDiff < 5.0f; 

        // Edge Detection Raycast
        float lookDir = (target.position.x > transform.position.x) ? 1f : -1f;
        Vector2 checkOrigin = (Vector2)transform.position + new Vector2(lookDir * edgeLookAhead, 0);
        bool groundAhead = Physics2D.Raycast(checkOrigin, Vector2.down, edgeLookDown, groundLayer);
        bool aboutToFall = !groundAhead; // True if we are at an edge

        // Movement Decision Tree

        if (targetIsBelow)
        {
            // 1. DROP DOWN: Target is way below. Ignore edges and run off the cliff.
            MoveTowardsX(target.position.x);
        }
        else 
        {
            // 2. CLIMB OR CROSS: Target is Above OR Same Height.
            // We must Jump if:
            // A) We are running out of floor (Gap ahead)
            // B) We are climbing (Target Above) AND we are close enough
            
            bool shouldJump = aboutToFall; // Gap crossing safety

            if (targetIsAbove && isCloseEnoughToJump) 
            {
                shouldJump = true; // Intentional climb jump
            }

            if (shouldJump)
            {
                StopMoving();
                StartCoroutine(PerformCalculatedJump(target.position));
            }
            else
            {
                // Run closer
                MoveTowardsX(target.position.x);
            }
        }

        // Arrival Check
        // (Only check arrival if we are on the ground and not intentionally jumping)
        if (dist <= waypointReachedDist && !targetIsAbove && !_isJumping)
        {
            if (_combatWaypoints.Contains(target)) 
            {
                EnterCombatState();
            }
            else 
            {
                _currentWaypointIndex++;
            }
        }
    }
    
    void MoveTowardsX(float targetX)
    {
        float dirX = (targetX > transform.position.x) ? 1f : -1f;
        _rb.velocity = new Vector2(dirX * runSpeed, _rb.velocity.y);
        animator?.SetFloat("Speed", 1f);
    }

    IEnumerator PerformCalculatedJump(Vector3 targetPos)
    {
        _isJumping = true;
        _isWaiting = true;
        _jumpLogIndex++;
        
        animator?.SetTrigger("Jump");
        
        yield return new WaitForSeconds(0.2f); 

        float distance = Vector2.Distance(transform.position, targetPos);
        float dynamicJumpTime = Mathf.Clamp(distance * 0.12f, 0.5f, 1.1f);

        Vector2 velocity = CalculateJumpVelocity(transform.position, targetPos, dynamicJumpTime);
        _rb.velocity = velocity;

        yield return new WaitForSeconds(dynamicJumpTime);

        _rb.velocity = Vector2.zero;
        transform.position = Vector2.Lerp(transform.position, targetPos, 0.5f);

        _isWaiting = false;
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator LandAndStabilize()
    {
        _isJumping = false;
        _isWaiting = true;
        StopMoving();

        if (_currentWaypointIndex < escapeWaypoints.Length)
        {
            Transform currentWP = escapeWaypoints[_currentWaypointIndex];
            if (currentWP != null)
            {
                if (_combatWaypoints.Contains(currentWP))
                {
                    yield return new WaitForSeconds(landedDelay);
                    _isWaiting = false;
                    EnterCombatState();
                    yield break; 
                }
                else
                {
                    _currentWaypointIndex++;
                }
            }
        }

        yield return new WaitForSeconds(landedDelay); 
        _isWaiting = false;
    }

    Vector2 CalculateJumpVelocity(Vector2 start, Vector2 end, float time)
    {
        Vector2 distance = end - start;
        Vector2 gravity = Physics2D.gravity * _rb.gravityScale;
        Vector2 initialVelocity = (distance - 0.5f * gravity * (time * time)) / time;
        return initialVelocity;
    }

    // Combat Logic

    void HandleCombat()
    {
        if (_player.position.y > transform.position.y + 3.0f && _playerMove != null && _playerMove.IsGrounded())
        {
            Debug.Log("[BOSS] Player climbed high above! Cancelling combat.");
            CancelCombatAndChase();
            return;
        }

        if (_isWaitingForPlayerOnPlatform)
        {
            float yDiffCurrent = transform.position.y - _player.position.y;
            
            if (Mathf.Abs(yDiffCurrent) < 1.5f) 
            {
                Debug.Log("[BOSS] Player arrived on platform! FIGHT!");
                _isWaitingForPlayerOnPlatform = false;
                
                if (_health) _health.SetCutsceneInvincibility(false);
                
                if (AggroIndicatorManager.Instance) AggroIndicatorManager.Instance.ShowAggro(transform);
                return;
            }

            PatrolPlatform();
            return;
        }

        _combatTimer -= Time.deltaTime;
        StandardCombatLogic(3f, false);

        if (_combatTimer <= 0 && !_isAttacking)
        {
            _currentWaypointIndex++; 
            EnterRunState();        
        }
    }

    void PatrolPlatform()
    {
        float lookDir = _patrolMovingRight ? 1f : -1f;
        Vector2 checkOrigin = (Vector2)transform.position + new Vector2(lookDir * 1.0f, 0);
        bool groundAhead = Physics2D.Raycast(checkOrigin, Vector2.down, 1.5f, groundLayer);

        if (!groundAhead)
        {
            _patrolMovingRight = !_patrolMovingRight;
        }

        float speed = 2.0f;
        float dirX = _patrolMovingRight ? 1f : -1f;
        
        _rb.velocity = new Vector2(dirX * speed, _rb.velocity.y);
        
        if (dirX > 0 && !_facingRight) Flip();
        else if (dirX < 0 && _facingRight) Flip();
        
        animator?.SetFloat("Speed", 1f);
    }

    void CancelCombatAndChase()
    {
        _isWaitingForPlayerOnPlatform = false;
        _currentWaypointIndex++; 
        EnterRunState();
    }

    void HandleLightningHell()
    {

        if (!_phase2CombatReady) return;

        if(!_rb.isKinematic) _rb.isKinematic = true;

        _phase2HoverTimer -= Time.deltaTime;
        if (_phase2HoverTimer <= 0)
        {
            float randomX = Random.Range(-4f, 4f);
            float randomY = Random.Range(3f, 6f); 
            _phase2HoverTarget = new Vector2(_player.position.x + randomX, _player.position.y + randomY);
            _phase2HoverTimer = 2.0f; 
        }

        Vector2 newPos = Vector2.MoveTowards(transform.position, _phase2HoverTarget, phase2MoveSpeed * Time.deltaTime);
        _rb.MovePosition(newPos);
        FaceTarget(_player.position);

        if (Time.time >= _phase2NextShootTime)
        {
            if(!_isAttacking) 
            {
                StartCoroutine(PerformBurstShot()); 
                _phase2NextShootTime = Time.time + phase2ShootInterval;
            }
        }
    }

    void StandardCombatLogic(float speed, bool isP2)
    {
        if (_isAttacking) return; 

        float dist = Vector2.Distance(transform.position, _player.position);
        FaceTarget(_player.position);

        if (dist <= 6.5f) 
        {
            StopMoving();
            if (Time.time >= _nextAttackTime) StartCoroutine(PerformSlash(isP2));
        }
        else if (dist <= 12f) 
        {
            float attackRoll = Random.value;
            if (Time.time >= _nextAttackTime && attackRoll < 0.4f)
            {
                StopMoving();
                StartCoroutine(PerformBlast(isP2));
            }
            else
                ChasePlayer(speed);
        }
        else
            ChasePlayer(speed);
    }

    void ChasePlayer(float speed)
    {
        float dirX = (_player.position.x > transform.position.x) ? 1f : -1f;
        _rb.velocity = new Vector2(dirX * speed, _rb.velocity.y);
        animator?.SetFloat("Speed", 1f);
    }

    void StopMoving()
    {
        _rb.velocity = new Vector2(0f, _rb.velocity.y);
        animator?.SetFloat("Speed", 0f);
    }

    void FaceTarget(Vector3 targetPos)
    {
        if (targetPos.x > transform.position.x && !_facingRight) Flip();
        else if (targetPos.x < transform.position.x && _facingRight) Flip();
    }

    void Flip()
    {
        _facingRight = !_facingRight;
        if (visual) {
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x) * (_facingRight ? 1f : -1f);
            visual.localScale = scale;
        }
    }

    void IgnoreMyColliders(GameObject projectile)
    {
        Collider2D[] projCols = projectile.GetComponentsInChildren<Collider2D>();
        foreach (var myCol in _allMyColliders)
        {
            if (myCol == null) continue;
            foreach (var projCol in projCols) Physics2D.IgnoreCollision(myCol, projCol);
        }
    }

    void HandleDeath()
    {
        if (_isDead) return;
        _isDead = true;

        StopAllCoroutines(); 
        
        FallingPlatform.hasGameStarted = false;

        InfiniteTower tower = FindObjectOfType<InfiniteTower>();
        if (tower != null) tower.isBossDead = true;
        // Stop moving horizontally, but allow vertical movement (falling)
        _rb.velocity = new Vector2(0, _rb.velocity.y); 
        
        // IMPORTANT: Set body to DYNAMIC so Gravity pulls him down
        _rb.bodyType = RigidbodyType2D.Dynamic;
        
        // Keep colliders ENABLED for now so he hits the floor
        foreach (var col in _allMyColliders) 
        {
            if (col) col.enabled = true;
        }

        Debug.Log("Boss Defeated! Falling to ground...");

        // Start the sequence: Fall -> Land -> Die Animation -> Destroy
        StartCoroutine(DeathFallSequence());
    }

    // 2. Add this NEW Coroutine:
    IEnumerator DeathFallSequence()
    {
        // 1. Wait until the boss hits the ground
        float timeout = 5f;
        while (!IsTouchingGround() && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        // 2. Freeze and play death animation
        _rb.velocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        foreach (var col in _allMyColliders) { if (col) col.enabled = false; }
        if (animator != null) animator.SetTrigger("Die");

        // 3. Start Dialogue and calculate how long it takes
        float totalDialogueTime = 2.0f; 
        if (bossDeathDialogue != null && SmartIslandController.Instance != null)
        {
            SmartIslandController.Instance.PlayDialogue(bossDeathDialogue);
            totalDialogueTime = 0f;
            foreach (var sentence in bossDeathDialogue.sentences)
            {
                float duration = (sentence.voiceOver != null) ? sentence.voiceOver.length : 2.0f;
                totalDialogueTime += (Mathf.Max(duration, 2.0f) + 0.5f);
            }
        }

        // 4. WAIT for the dialogue to finish before showing the victory screen
        yield return new WaitForSeconds(totalDialogueTime); 

        // 5. NOW trigger the Victory Sequence
        VictorySequenceManager victoryManager = FindObjectOfType<VictorySequenceManager>();
        if (victoryManager != null)
        {
            victoryManager.StartVictorySequence();
        }

        Destroy(gameObject);
    }

    // 3. Helper to check if we hit the floor
    bool IsTouchingGround()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void EnterRunState()
    {
        currentState = BossState.Running;
        if (_health) _health.SetCutsceneInvincibility(true);
    }

    void EnterCombatState()
    {
        StopMoving();
        currentState = BossState.Combat;
        _combatTimer = combatDuration;
        _isWaitingForPlayerOnPlatform = true;

        _rb.isKinematic = false; 
        _rb.bodyType = RigidbodyType2D.Dynamic;

        if (_health) _health.SetCutsceneInvincibility(true);
    }

    IEnumerator EnterPhase2LightningHell()
    {
        _hasTriggeredPhase2 = true;
        currentState = BossState.Phase2_LightningHell;
        _phase2CombatReady = false; 
        
        StopMoving();
        _rb.velocity = Vector2.zero;

        if (_health) _health.SetCutsceneInvincibility(true); 
        
        Debug.Log("[BOSS] Phase 2 Starting!");

        // Switch to Phase 2 music
        if (phase2Music != null && MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(phase2Music);
        }

        // 1. THE DRAMATIC PAUSE (Slow Motion)
        Time.timeScale = 0.2f; 
        animator?.SetBool("IsPhase2", true); 
        
        // Stop infinite tower generation and spawn the roof
        InfiniteTower tower = FindObjectOfType<InfiniteTower>();
        if (tower != null) 
        {
            tower.isBossDead = true;
            tower.SpawnRoof();
        }

        yield return new WaitForSecondsRealtime(1.0f); // Tension wait

        // 2. RESUME TIME
        Time.timeScale = 1.0f;
        
        // 3. DESTROY OLD PLATFORMS
        if (destroyPlatformsOnPhase2)
        {
            GameObject[] platforms = GameObject.FindGameObjectsWithTag("GamePlatform");
            foreach (GameObject platform in platforms)
            {
                if (platformDestructionVFX != null)
                    Instantiate(platformDestructionVFX, platform.transform.position, Quaternion.identity);
                Destroy(platform);
            }
        }

        // 4. SPAWN THE ARENA
        if (arenaFloorPrefab != null && _spawnedArena == null)
        {
            Vector3 spawnPos = new Vector3(_player.position.x, _player.position.y - 8f, 0);
            _spawnedArena = Instantiate(arenaFloorPrefab, spawnPos, Quaternion.identity);
        }

        _rb.isKinematic = true;
        
        if (AggroIndicatorManager.Instance) 
            AggroIndicatorManager.Instance.ShowAggro(transform);
        
        // 5. CAMERA & FLOATING & ANIMATION (SYNCED!)
        LevelCameraPreview cameraScript = FindObjectOfType<LevelCameraPreview>(true);
        if (cameraScript != null)
        {
            cameraScript.enabled = true;
            Coroutine cameraFollow = StartCoroutine(cameraScript.FollowMovingTarget(transform, 4f));
            
            // Trigger animation when upward movement starts
            animator?.SetTrigger("TransformNow");

            // Move Boss Upwards (The Floating)
            float elapsed = 0f;
            float startY = transform.position.y;
            float targetY = startY + 5f;
            
            // Floating duration (2 seconds)
            while (elapsed < 2f)
            {

                float t = elapsed / 2f;
                t = t * t * (3f - 2f * t); // SmoothStep easing
                
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(startY, targetY, t), 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            yield return cameraFollow;
        }
        else
        {
            // Fallback if no camera script
            animator?.SetTrigger("TransformNow");
            yield return new WaitForSeconds(1.0f);
        }
        
        // 6. END SCENE, START FIGHT
        if (_health) _health.SetCutsceneInvincibility(false);
        _phase2NextShootTime = Time.time + 2f; 
        _phase2CombatReady = true; 
        StartCoroutine(SpawnLightningRoutine());
        
        Debug.Log("[BOSS] Phase 2 fully active!");
    }

    IEnumerator PerformSlash(bool isP2)
    {
        _isAttacking = true;
        StopMoving();
        animator?.SetTrigger("Attack_Melee");
        yield return new WaitForSeconds(0.15f);

        Vector2 lungeDir = _facingRight ? Vector2.right : Vector2.left;
        float travelDistance = 20f * 0.3f; 
        Vector2 checkPos = (Vector2)transform.position + lungeDir * travelDistance;
        checkPos.y -= 1f; 
        bool groundAhead = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);
        if (!groundAhead) travelDistance = 2f; 

        RaycastHit2D[] pathHits = Physics2D.BoxCastAll(meleeAttackPoint.position, new Vector2(meleeRadius, meleeRadius), 0f, lungeDir, travelDistance);
        foreach (var hit in pathHits) {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
                hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(meleeDamage);
        }

        float elapsed = 0f;
        float lungeSpeed = groundAhead ? 20f : 8f; 
        while (elapsed < 0.3f) {
            _rb.velocity = new Vector2(lungeDir.x * lungeSpeed, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopMoving();
        yield return new WaitForSeconds(0.4f);
        _nextAttackTime = Time.time + (isP2 ? 1f : 2f);
        _isAttacking = false;
    }

    IEnumerator PerformBlast(bool isP2)
    {
        _isAttacking = true;
        StopMoving();
        animator?.SetTrigger("Attack_Ranged");
        yield return new WaitForSeconds(0.5f);

        GameObject bulletPrefab = isP2 ? bulletPrefabPurple : bulletPrefabRed;
        if (bulletPrefab && firePoint) {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            IgnoreMyColliders(bullet);
            Vector2 dir = (_player.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            if (bullet.GetComponent<Rigidbody2D>()) bullet.GetComponent<Rigidbody2D>().velocity = dir * 15f;
        }
        yield return new WaitForSeconds(0.5f);
        _nextAttackTime = Time.time + rangedCooldown;
        _isAttacking = false;
    }

    IEnumerator PerformBurstShot()
    {
        _isAttacking = true;
        for (int i = 0; i < phase2BurstCount; i++) {
            animator?.SetTrigger("Attack_Ranged");
            yield return new WaitForSeconds(0.3f);
            if (bulletPrefabPurple && firePoint) {
                GameObject bullet = Instantiate(bulletPrefabPurple, firePoint.position, Quaternion.identity);
                IgnoreMyColliders(bullet);
                Vector2 dir = (_player.position - firePoint.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                if (bullet.GetComponent<Rigidbody2D>()) bullet.GetComponent<Rigidbody2D>().velocity = dir * 15f;
            }
            yield return new WaitForSeconds(phase2BurstDelay);
        }
        _isAttacking = false;
    }

    // Fixed Spawn Logic
    IEnumerator SpawnLightningRoutine()
    {
        while (currentState == BossState.Phase2_LightningHell && _health.GetHealthPercent() > 0)
        {
            // 1. Pick Random Offset
            float offsetX = Random.Range(-lightningSpawnDistanceX, lightningSpawnDistanceX);
            
            // 2. Calculate Spawn Position (Sky)
            Vector2 spawnPos = new Vector2(_player.position.x + offsetX, _player.position.y + 5.0f);
            
            // 3. SMART WARNING SYSTEM
            // Default to spawning in the sky if no ground is found
            Vector2 warningPos = spawnPos;

            // Shoot a ray through EVERYTHING below the spawn point
            RaycastHit2D[] hits = Physics2D.RaycastAll(spawnPos, Vector2.down, 30f, groundLayer);

            if (hits.Length > 0)
            {
                // Find the platform that is vertically closest to the player's feet
                // This ignores roofs way above the player and pits way below
                RaycastHit2D closestHit = hits[0];
                float closestDiff = Mathf.Abs(closestHit.point.y - _player.position.y);

                foreach (RaycastHit2D hit in hits)
                {
                    float diff = Mathf.Abs(hit.point.y - _player.position.y);
                    if (diff < closestDiff)
                    {
                        closestDiff = diff;
                        closestHit = hit;
                    }
                }
                warningPos = closestHit.point;
            }

            // 4. Show Warning
            if (AggroIndicatorManager.Instance) 
                AggroIndicatorManager.Instance.ShowAggroAtPosition(warningPos);
            
            yield return new WaitForSeconds(0.8f); // Warning Duration

            // 5. Spawn Lightning
            if (lightningStrikePrefab)
            {
                Quaternion spawnRot = Quaternion.identity;
                GameObject lightning = Instantiate(lightningStrikePrefab, spawnPos, spawnRot);
                IgnoreMyColliders(lightning);
            }
            
            yield return new WaitForSeconds(lightningSpawnRate);
        }
    }
}