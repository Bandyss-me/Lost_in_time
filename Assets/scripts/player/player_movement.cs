using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class player_movement : NetworkBehaviour
{
    private CharacterController playerController;
    private Animator _animator;

    [Header("Movement")]
    [SerializeField]
    float walk_speed=5f;
    [SerializeField]
    float run_speed=10f;
    [SerializeField]
    float air_speed=2f;

    [Header("Jumping")]
    [SerializeField]
    float jumpForce=4f;
    [SerializeField]
    float jumpTimeLimit=0.4f;

    [Header("Camera")]
    [SerializeField]
    Transform playerCamera;
    [SerializeField]
    float mouseSensitivity=0.1f;

    [Header("Input")]
    [SerializeField]
    InputActionAsset inputActionAsset;

    private InputAction move_input;
    private InputAction look_input;
    private InputAction jump_input;
    private InputAction sprint_input;

    //other
    bool _hasGravity=true;
    bool _isGrounded;
    bool _isInAir;

    //jumping
    bool _hasLanded;
    bool _hasJumped;
    float _heigthTimer;

    float verMov;
    float _cameraPitch;
    Vector3 _movementV;
    Vector3 _prevMovementV;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        playerController=GetComponent<CharacterController>();
        move_input = inputActionAsset.FindAction("Move");
        look_input = inputActionAsset.FindAction("Look");
        jump_input = inputActionAsset.FindAction("Jump");
        sprint_input = inputActionAsset.FindAction("Sprint");
        inputActionAsset.Enable();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerCamera.gameObject.SetActive(IsOwner);
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        inputActionAsset.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible=false;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _hasLanded = false;
        _hasJumped = false;
    }

    void Update(){
        GroundCheck();
        Move();
        RotateWithMouse();
    }

    void GroundCheck(){
        _isGrounded = playerController.isGrounded;
    }

    void Newton_is_watching_you(){
        if(_hasGravity){
            if (!_isGrounded){
                verMov-=9.8f*Time.deltaTime;
            }
            else{
                verMov=0;
            }
        }
    }

    void Move(){
        Vector2 input=move_input.ReadValue<Vector2>().normalized;
        float tspeed=sprint_input.IsPressed()?run_speed:walk_speed;
        if (_isGrounded)
        {
            if (input != Vector2.zero)
            {
                if (sprint_input.IsPressed())
                    _animator.SetInteger("state", 2);
                else _animator.SetInteger("state", 1);
            }
            else _animator.SetInteger("state", 0);
        }
        Vector3 dir = transform.right*input.x + transform.forward*input.y;
        _movementV.x=dir.x*tspeed;
        _movementV.z=dir.z*tspeed;
        Jump();
        Newton_is_watching_you();
        _movementV.y+=verMov;
        _movementV*=Time.deltaTime;
        if(_isInAir && false){
            _movementV.x=_prevMovementV.x;
            _movementV.z=_prevMovementV.z;
        }
        playerController.Move(_movementV);
        _prevMovementV=_movementV;
    }

    void RotateWithMouse(){
        Vector2 look = look_input.ReadValue<Vector2>() * mouseSensitivity;
        transform.Rotate(Vector3.up * look.x);
        _cameraPitch -= look.y;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -90f, 90f);
        playerCamera.transform.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
    }

    void Jump(){
        if(_isGrounded && !_hasLanded)
        {
            _animator.ResetTrigger("jump");
            _hasLanded=true;
        }
        bool jumpHeld = jump_input.IsPressed();
        if (jumpHeld && _hasLanded)
        {
            _animator.SetTrigger("jump");
            verMov=0f;
            verMov+=jumpForce;
            _heigthTimer=0f;
            _hasJumped=true;
            _hasGravity=false;
            _hasLanded=false;
        }
        if (jumpHeld && _heigthTimer<jumpTimeLimit)
        {
            _heigthTimer+=Time.deltaTime;
            verMov-=Time.deltaTime;
        }
        else if (_hasJumped)
        {
            _hasJumped=false;
            if(!_hasGravity)
                _hasGravity=true;
        }
    }
}