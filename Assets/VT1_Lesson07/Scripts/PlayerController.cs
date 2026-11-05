using Unity.VisualScripting.Antlr3.Runtime;
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
        // 移動処理
        Movement();

        // Rayの作成
        Ray ray1 = MakeRay(transform, Vector3.up * -1f);
        bool isHit = TryGetRaycast(ray1, 10f, out RaycastHit hit);
        if (isHit)
        {
            // Raycastでの衝突が発生した場合
            Debug.Log($"{ray1} が衝突を検知しました。");

            // 法線(normal)の向きを可視化
            Debug.DrawRay(hit.point, hit.normal, Color.blue, 0, false);

            // プレイヤーの正面ベクトルを可視化
            Debug.DrawRay(transform.position, transform.forward, Color.yellow, 0, false);

            // 内積の計算
            var dot = Vector3.Dot(transform.forward, hit.normal);
            Debug.Log($"内積： {dot} ");

            // 外積の計算
            var cross = Vector3.Cross(transform.forward, hit.normal);
            Debug.DrawRay(transform.position, cross, Color.yellowGreen, 0, false);
;        }
    }

    /// <summary>
    /// 移動処理メソッド
    /// </summary>
    public void Movement()
    {
        var inputAxis = new Vector3(_inputMoveVector.x, 0, _inputMoveVector.y);

        var cameraForward = _cameraController.transform.forward;

        var moveVector = (inputAxis.x * _cameraController.transform.right) + (inputAxis.z * cameraForward);
        moveVector.y = 0;

        moveVector = GetSlopeVector(moveVector);

        _velocity = inputAxis;

        Vector3 finalVelocity = _moveSpeed * _velocity * Time.deltaTime;
        transform.Translate(finalVelocity, Space.World);
    }

    #region 入力関連

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

    #endregion

    #region Raycast関連

    private Ray MakeRay(Transform rayPoint, Vector3 direction)
    {
        // Rayを作る
        // Ray(1:始点, 2:方向ベクトル, 3:長さ)
        Ray ray = new Ray(rayPoint.position + Vector3.up * 0.2f, direction);

        // Rayを可視化
        // Debug.DrawRay(1:始点, 2:方向, 3:色, 4:持続時間, 5:深度描画)
        Debug.DrawRay(rayPoint.position + Vector3.up * 0.2f, direction, Color.red, 0, false);

        // 作成したRayを返す
        return ray;
    }

    /// <summary>
    ///  Rayを使った衝突判定
    /// </summary>
    private bool TryGetRaycast(Ray ray, float distance, out RaycastHit hit)
    {
        // RayCastで判定を行う
        // RayCast(1:Ray, 2:衝突対象の情報. 3:距離, 4:レイヤーマスク)
        return Physics.Raycast(ray, out hit, distance, LayerMask. GetMask("Default"));
    }

    private Vector3 GetSlopeVector(Vector3 vel)
    {
        Ray terrainRay = MakeRay(transform, Vector3.up * -1f);
        bool raycast = TryGetRaycast(terrainRay, 10f, out RaycastHit hit);

        if(raycast)
        {
            if (vel.sqrMagnitude < 0.1f) return vel;

            Vector3 normal = hit.normal;
            Vector3 point = hit.point;

            // 移動先のベクトルを可視化
            Debug.DrawRay(transform.position, vel, Color.yellow, 0, false);

            // 法線のベクトルを可視化
            Debug.DrawRay(transform.position, normal, Color.blue, 0, false);

            // 外積で「進行方向」と「法線の向き」に垂直のベクトル
            Vector3 crossRight = Vector3.Cross(vel, normal);
            Debug.DrawRay(transform.position, crossRight, Color.yellowGreen, 0, false);

            // さらに外積で「垂直のベクトル(右向き)」と「法線の向き」に垂直のベクトル(正面)
            Vector3 crossForward = Vector3.Cross(crossRight, normal);
            Debug.DrawRay(transform.position, crossForward, Color.white, 0, false);

            // 地形の角度を求める
            float dot = Vector3.Dot(vel, normal);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            Debug.Log($"傾斜の角度： {angle} ");

            vel = crossForward;
        }

        return vel;
    }

    #endregion
}
