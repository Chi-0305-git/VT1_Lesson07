using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("＊＊＊移動設定＊＊＊")]
    [SerializeField] private float _moveSpeed;

    [Header("＊＊＊カメラ設定＊＊＊")]
    [SerializeField] private CameraController _cameraController;

    [Header("＊＊＊Player Input＊＊＊")]
    [SerializeField] private PlayerInput _playerInput;

    //=== Private変数 ===
    private Vector3 _velocity;
    private Vector3 _inputMoveVector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MoveInputEnable();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }


    public void Movement()
    {
        var inputAxis = new Vector3(_inputMoveVector.x, 0, _inputMoveVector.y);

        var cameraForward = _cameraController.transform.forward;

        var moveVector = (inputAxis.x * _cameraController.transform.right) + (inputAxis.z * cameraForward);
        moveVector.y = 0;

        _velocity = inputAxis;

        Vector3 finalVelocity = _moveSpeed * _velocity * Time.deltaTime;
        transform.Translate(finalVelocity, Space.World);
    }

    public void MoveInputEnable()
    {
        if (_playerInput == null)
        {
            Debug.LogError("PlayerInputが設定されていません。");
        }

        //移動入力の設定登録
        _playerInput.actions["Move"].performed += context =>
        {
            //入力時の処理 
            var value = context.ReadValue<Vector2>();     //入力値の取得
            _inputMoveVector = value;                   //変数に保存
        };

        _playerInput.actions["Move"].canceled += context =>
        {
            _inputMoveVector = Vector2.zero;    //変数の値をリセット
        };

        //アクションを有効化する
        _playerInput.actions["Move"].Enable();

    }
}
