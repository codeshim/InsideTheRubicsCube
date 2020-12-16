using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO.Ports;
using System;

public enum PlayerState
{
    Horizontal,
    Vertical,
}

public class PlayerCtrl : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM3", 9600); //set Serial port

    Transform tr = null;            // Transform 컴포넌트 빠르게 접근하기 위한 변수
    float h = 0.0f;                 // 수평방향 변수(1은 오른쪽 -1은 왼쪽)
    float v = 0.0f;                 // 수직방향 변수(1은 위 -1은 아래)
    float moveSpeed = 0.4f;         // 이동 속도
    float rotSpeed = 150.0f;        // 회전 속도
    int rot = 1;
    float targetY = 0.0f;           // 45도 회전 타겟 방향
    float tgCacY = 0.0f;            // 타겟 회전 계산 변수
    bool rotDone = true;            // 회전이 끝났는지
    bool resetOn = false;           // 리셋 실행을 위해 버튼을 누른 상태인지
    float resetSec = 3.0f;          // 리셋 실행을 위해 버튼을 누르고 대기하는 시간
    float coolSec = 1.0f;           // 씬 리로드 후 회전상태가 되지 않게
    GameObject curGround = null;    // 현재 소속된 parent 바닥
    Vector3 prevVec = Vector3.zero; // 바닥을 벗어나기 이전 위치

    float vRayLen = 0.03f;          // 바닥으로 쏘는 raycast 길이
    float hRayLen = 0.15f;          // 앞으로 쏘는 raycast 길이(사다리 판별하기 위함)
    Vector3 cacVec = Vector3.zero;  // 거리 계산하기 위한 벡터 변수
    float cacLen = 0.0f;            // 계산된 거리

    float dist = 0.2f;              // 수평이동에서 수직이동으로 바뀌기 위한 사다리와의 거리
    float height = 0.02f;           // 플레이어가 바닥에서부터 띄어있는 높이
    float prevHeight = 0.0f;        // 플레이어가 사다리를 타고 내려가거나 올라가기 전 높이
    bool AskOn = true;
    bool landOk = false;            // 플레이어가 사다리를 타기 이전 바닥에 도착해도 되는지의 여부
    //bool changeOk = true;           // 플레이어가 내려섰을 때 이동하기 전까지 상태변환 방지
    Vector3 landVec = Vector3.zero; // 사다리에서 내려섰을 때의 위치

    // 플레이어의 이동상태 변수
    public PlayerState playerState = PlayerState.Horizontal;

    // 초기화를 위한 변수
    GameObject firstGround = null;
    Vector3 firstVec = Vector3.zero;
    Quaternion firstRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {

        sp.Open();
        sp.ReadTimeout = 5;

        tr = GetComponent<Transform>();

        // 초기위치 지정
        if (firstGround == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, vRayLen) == true)
            {
                firstGround = hit.collider.gameObject;
                this.transform.SetParent(firstGround.transform);
                firstVec = hit.point;
                firstRot = tr.rotation;
                height = this.transform.position.y - firstVec.y;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!rotDone)
        {
            Rotate();
        }

        Swipe();

        if (sp.IsOpen)
        {
            try
            {
                if (coolSec > 0.0f)
                {
                    SceneMgr.Inst.LoadingTxt();
                    coolSec -= Time.deltaTime;
                }
                else
                {
                    SceneMgr.Inst.ChangeStateTxt(playerState);

                    RotateKey(sp.ReadByte());

                    if (rotDone)
                    {
                        Move(sp.ReadByte());
                    }

                    Escape(sp.ReadByte());
                }
                
            }
            catch (System.Exception)
            {
                Debug.Log("e");
                throw;
            }
            Debug.Log("connected");
        }

        else if (!sp.IsOpen)
        {
            Debug.Log("not connected");
        }
    }


    // 갇혀서 움직이지 못할 때 맨 초기 위치로 바꾸는 함수
    void Escape(int num3)
    {
        // 리셋
        resetOn = num3 == 6 && num3 == 4 ? true:false;

        if (resetOn)
        {
            resetSec -= Time.deltaTime;

            if (SceneMgr.Inst.StateTxt != null)
            {
                SceneMgr.Inst.StateTxt.text = "- Reset... " + resetSec.ToString("F0");
            }

            if (resetSec < 0.0f)
            {
                // 현재 씬 리로드(씬 이름이 변경된다면 변경해서 넣기)
                SceneManager.LoadScene("InScene");
            }
        }
        else
        {
            resetSec = 3.0f;    // 리셋 타임 초기화
        }
    }

    // 이동값을 받아 이동시키는 함수
    void Move(int num1)
    {
        // 수평이동상태
        if (playerState == PlayerState.Horizontal)
        {
            RaycastHit hit;
            // 높이를 항상 일정하게 유지
            if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, vRayLen + 0.5f) == true
                 && tr.position.y - hit.point.y != height)
            {
                tr.position = new Vector3(tr.position.x, (hit.point.y + height), tr.position.z);
            }

            if (num1 == 1 && !SceneMgr.Inst.AskPanel.activeSelf)
                h = 1.0f;
            else if (num1 == 3 && !SceneMgr.Inst.AskPanel.activeSelf)
                h = -1.0f;
            else if (num1 == 1 || num1 == 3)
            {
                SwipeKey(num1);
            }

            if (num1 == 5)
                v = 1.0f;
            else if (num1 == 2)
                v = -1.0f;
            else 
                v = 0.0f;

            if (h != 0 || v != 0)
            {
                Debug.DrawRay(this.transform.position, -this.transform.up * vRayLen, Color.white, 10f);

                // 바닥이 인식되어야 이동한다. 수평적으로 전진
                if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, vRayLen) == true)
                {
                    Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
                    tr.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
                }
            }
        }

        // 수직이동상태
        if (playerState == PlayerState.Vertical)
        {
            if (num1 == 1 || num1 == 3)
            {
                SwipeKey(num1);
            }

            if (num1 == 5)
                v = 1.0f;
            else if (num1 == 2)
                v = -1.0f;
            else
                v = 0.0f;

            if (v != 0)
            {
                // 수직적으로 상승
                Vector3 moveDir = Vector3.up * v;
                tr.Translate(moveDir.normalized * (moveSpeed - 0.2f) * Time.deltaTime);
            }
        }
    }

    // 회전값을 받아 회전시키는 함수
    void RotateKey(int num2)
    {
        // 수직이동상태와 리셋상태에서 회전하지 못하게
        if (playerState == PlayerState.Vertical || resetOn)
            return;

        // 회전
        if (num2 == 6 && rotDone)
        {
            targetY = tr.eulerAngles.y + 45f;
            rot = 1;
            rotDone = false;
        }
        else if (num2 == 4 && rotDone)
        {
            targetY = tr.eulerAngles.y - 45f;
            if (targetY < 0)
            {
                targetY = 360 + targetY;
            }
            rot = -1;
            rotDone = false;
        }
    }

    void Rotate()
    {
        tgCacY = tr.eulerAngles.y + (rot * rotSpeed * Time.deltaTime);

        if ((rot == 1 && tgCacY >= targetY) || (rot == -1 && tgCacY <= targetY))
        {
            tgCacY = targetY;
            rotDone = true;
        }

        tr.eulerAngles = new Vector3(0.0f, tgCacY, 0.0f);
    }

    void SwipeKey(int num4)
    {
        RaycastHit hit;

        if (playerState == PlayerState.Horizontal && SceneMgr.Inst.AskPanel.activeSelf)
        {
            Debug.Log("In");
            if (num4 == 3 && AskOn)
            {
                // 사다리로 parent swipe하고 상태변환
                prevHeight = tr.position.y;
                Physics.Raycast(this.transform.position, this.transform.forward, out hit, hRayLen);
                curGround = hit.collider.gameObject;
                this.transform.SetParent(curGround.transform);
                playerState = PlayerState.Vertical;
            }
            else if (num4 == 1)
            {
                SceneMgr.Inst.AskEnd();
                AskOn = false;
            }
        }
        else if (playerState == PlayerState.Vertical && SceneMgr.Inst.AskPanel.activeSelf)
        {
            if (num4 == 3 && AskOn)
            {
                // 새로운 바닥이거나 이전 바닥에 도착해도 되는 상태일 때 바닥으로 parent swipe하고 상태변환
                Physics.Raycast(this.transform.position, -this.transform.up, out hit, vRayLen);
                curGround = hit.collider.gameObject;
                landVec = new Vector3(hit.point.x, hit.point.y + height, hit.point.z);
                tr.position = landVec;
                this.transform.SetParent(curGround.transform);
                playerState = PlayerState.Horizontal;
                landOk = false;
            }
            else if (num4 == 1)
            {
                SceneMgr.Inst.AskEnd();
                AskOn = false;
            }
        }
    }

    // 소속 parent를 변경하고 이동상태를 변경하는 함수
    void Swipe()//int num4)
    {
        RaycastHit hit;

        // 수평이동상태
        if (playerState == PlayerState.Horizontal)
        {
            if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, vRayLen) == true)
            {
                // 기존 바닥과 다른 바닥에 위치했을 때 parent swipe 
                if (hit.collider.gameObject != curGround)
                {
                    curGround = hit.collider.gameObject;
                    this.transform.SetParent(curGround.transform);
                    //Debug.Log(curGround);
                }

                // 바닥 위에 있을 때의 위치정보 받아놓기
                prevVec = this.transform.position;
            }
            else
            {
                // 바닥 위를 벗어났을 때 바로 이전 위치로 변경
                tr.position = prevVec;
            }

            // 사다리를 향하고 있고 사다리와 가까워졌을 때 수직이동상태로 변경
            if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, hRayLen) &&
                hit.collider.tag.Equals("Ladder")) //&& changeOk)
            {
                cacVec = hit.point - this.transform.position;
                cacLen = cacVec.magnitude;
                cacVec.y = 0.0f;

                if (cacLen < dist)
                {
                    if (AskOn)
                        SceneMgr.Inst.Ask(playerState);
                }
            }
            else
            {
                SceneMgr.Inst.AskEnd();
                AskOn = true;
            }
        }

        // 수직이동상태
        if (playerState == PlayerState.Vertical)
        {
            // 플레이어가 사다리를 타기 이전 바닥에 도착해도 되는지의 여부 판단
            if (Mathf.Abs(prevHeight - tr.position.y) > 0.2f && !landOk)
            {
                landOk = true;
                Debug.Log("landOk : " + landOk);
            }

            if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, hRayLen))
            {
                // 사다리를 올라가거나 내려가고 있을 때 위치정보 받아놓기
                prevVec = tr.position;
            }
            else
            {
                // 사다리 끝에 아무것도 없는 경우 위로 무한으로 올라가지 않게 하기 위한 예외처리
                tr.position = prevVec;
            }

            // 수직이동 중에 바닥에 올라설 때 플레이어가 수평이동 상태로 바뀌게 한다
            if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, vRayLen) &&
                     hit.collider.gameObject != curGround && landOk)
            {
                if (AskOn)
                    SceneMgr.Inst.Ask(playerState);
            }
            else
            {
                SceneMgr.Inst.AskEnd();
                AskOn = true;
            }
        }
    }
}

