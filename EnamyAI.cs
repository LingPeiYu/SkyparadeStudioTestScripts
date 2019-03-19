using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnamyAI : MonoBehaviour
{
    //新回合开始位置
    public Vector2 startPosition;
    //基点范围
    public float pointOffset;
    //地面纵坐标
    public float groundY;
    //行为用变量
    public float jumpVelocity;//纵跳向上初速度
    public float hitUpVelocity;//扑击横向初速度
    public float hitForwardVelocity;//扑击纵向初速度
    public AudioSource jumpSound;//跳跃音效
    public GameObject jumpEffect;//跳跃效果
    public float speed;//移动速度
    //状态用变量
    public float middle;//场地中心x坐标
    public float playerJumpLine;//高度线判断玩家是否起跳
    public float alarmLine;//在玩家移动区域设置横向线判断玩家是否近墙
    public float alarmHeight;//警戒高度判断是否进入击球状态
    public Transform playerTransform;//监视玩家位置
    public Transform ballTransform;//监视球的位置
    public Rigidbody2D ballRigidbody;//监视球的物理状态
    //纵跳击球区域各偏移量绝对值
    public float jumpAreaFrontOffset;
    public float jumpAreaAfterOffset;
    public float lineBetweenFrontAndMiddleOffset;
    public float lineBetweenMiddleAndBackOffset;


    //纵跳击球横向区域（左前右后）
    private float jumpAreaFrontX;
    private float jumpAreaAfterX;
    private float lineBetweenFrontAndMiddle;
    private float lineBetweenMiddleAndBack;

    private Rigidbody2D rigidbody2d;
    private Animator animator;
    private bool isJumping;//标记是否处于跳跃中
    private float g;//重力加速度
    //枚举，定义状态
    private enum State
    {
        Idle,
        NearWallDefence,
        Alarm,
        Prepare,
        ReadyToHit,
    }
    private State curState;

    //初始化
    public void InitializeState()
    {
        curState = State.Idle;
        transform.position = startPosition;
        isJumping = false;
        rigidbody2d.velocity = Vector2.zero;
    }

    private void StateUpdate()
    {
        //判断当前状态，调用相应函数
        switch (curState)
        {
            case State.Idle:
                IdleUpdate();
                break;
            case State.NearWallDefence:
                NearWallDefenseUpdate();
                break;
            case State.Alarm:
                AlarmUpdate();
                break;
            case State.Prepare:
                PrepareUpdate();
                break;
            case State.ReadyToHit:
                ReadyToHitUpdate();
                break;
        }
    }

    //状态函数
    private void IdleUpdate()
    {
        //闲置等待，回到场地中心准备应付来球
        if (transform.position.x > middle + pointOffset)
            MoveForward();
        else if (transform.position.x < middle - pointOffset)
            MoveBack();
        else
            Stop();
        //根据条件改变状态
        if (ballTransform.position.x > 0)//球过线
            curState = State.Alarm;
        else if (playerTransform.position.x > alarmLine && playerTransform.position.y < playerJumpLine
            && ballTransform.position.x >= playerTransform.position.x)//玩家可能打算近墙击球
            curState = State.NearWallDefence;
    }

    private void NearWallDefenseUpdate()
    {
        //近墙准备防守来球，以玩家与墙间距为基点
        float standOffset = 0 - playerTransform.position.x;
        float curOffset = transform.position.x - 0;
        if (curOffset > standOffset + pointOffset)
            MoveForward();
        else if (curOffset < standOffset - pointOffset)
            MoveBack();
        else
            Stop();
        //根据条件改变状态
        if (ballTransform.position.x > 0)//球过线
            curState = State.Alarm;
        else if (playerTransform.position.x < alarmLine)//玩家离开近墙防守警戒距离
            curState = State.Idle;
    }

    private void AlarmUpdate()
    {
        //警戒，根据球的状态调整状态
        if (ballTransform.position.x < 0)//球已经打回去了
            curState = State.Idle;
        else
        {
            if (CalculatePlacement() > 0)//球的落点在己区
            {
                if (ballTransform.position.y > alarmHeight)//球在警戒高度之上
                    curState = State.Prepare;
                else if (ballTransform.position.y <= alarmHeight)//球警戒高度或之下
                    curState = State.ReadyToHit;
            }
            else
                Stop();//其他情况不动并保持警戒
        }
    }

    private void PrepareUpdate()
    {
        //根据落点调整位置准备击球
        float placement = CalculatePlacement();//获得落点横坐标
        if (transform.position.x < placement - pointOffset)
            MoveBack();
        else if (transform.position.x > placement + pointOffset)
            MoveForward();
        else
            Stop();
        //根据条件改变状态
        if (ballTransform.position.y <= alarmHeight)//球警戒高度或之下
            curState = State.ReadyToHit;
    }

    private void ReadyToHitUpdate()
    {
        //击球就绪，移动到适合击球的位置
        float distanceFromJumpArea = DistanceFromJumpArea();//获得球距离纵跳击球区域的横向距离
        if (distanceFromJumpArea < 0)//在区域前
            MoveForward();
        else if (distanceFromJumpArea > 0)//在区域后
            MoveBack();
        else//在区域内
        {
            if (AboveBallLostHorizontalV())//判断正上方的球失去横向移动速度,后退，准备用前半部分击球
                MoveBack();
            else//球不在正上方或者没有失去横向移动速度
                Stop();//不动并保持击球就绪状态
        }
    }
    //用Trigger感知触发击球行为
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isJumping)//不处于jump中才能触发击球行为
        {
            if (DistanceFromJumpArea() > 0)//球在纵跳击球区域前，即进入的是扑击触发器，否则进入的是纵跳触发器
            {
                if (collision.GetComponent<Rigidbody2D>().velocity.y < 0)//下落时扑击
                    Hit();
            }
            else
                //纵跳击球
                Jump();
            //击球后回到警戒状态
            curState = State.Alarm;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isJumping)//不处于jump中才能触发击球行为
        {
            if (DistanceFromJumpArea() == 0)
                //纵跳击球
                Jump();
            //击球后回到警戒状态
            curState = State.Alarm;
        }
    }

    //行为函数
    private void MoveForward()
    {
        //向前移动
        if (!isJumping)
        {
            animator.SetBool("HaveSpeed", true);
            rigidbody2d.velocity = new Vector2(-speed, 0f);
        }
    }

    private void MoveBack()
    {
        //向后移动
        if (!isJumping)
        {
            animator.SetBool("HaveSpeed", true);
            rigidbody2d.velocity = new Vector2(speed, 0f);
        }
    }

    private void Jump()
    {
        //纵跳
        if (!isJumping)
        {
            isJumping = true;
            jumpEffect.transform.position = new Vector2(transform.position.x, jumpEffect.transform.position.y);
            jumpEffect.SetActive(true);
            jumpSound.Play();
            rigidbody2d.velocity = new Vector2(0f, jumpVelocity);
            animator.SetTrigger("Jump");
        }
    }

    private void Hit()
    {
        //扑击
        if (!isJumping)
        {
            isJumping = true;
            jumpEffect.transform.position = new Vector2(transform.position.x, jumpEffect.transform.position.y);
            jumpEffect.SetActive(true);
            jumpSound.Play();
            rigidbody2d.velocity = new Vector2(hitForwardVelocity, hitUpVelocity);
            animator.SetTrigger("Hit");
        }
    }

    private void Stop()
    {
        //静止
        if (!isJumping)
        {
            animator.SetBool("HaveSpeed", false);
            rigidbody2d.velocity = new Vector2(0f, 0f);
        }
    }

    //判断函数

    private bool AboveBallLostHorizontalV()
    {
        //判断正上方的球是否失去横向移动速度(绝对值<0.1)
        if (ballTransform.position.x > lineBetweenFrontAndMiddle && ballTransform.position.x < lineBetweenMiddleAndBack
            && Mathf.Abs(ballRigidbody.velocity.x) < 0.1)
            return true;
        else
            return false;
    }

    //计算函数
    private float CalculatePlacement()
    {
        //计算落点的横坐标
        float gOfBall = Mathf.Abs(Physics2D.gravity.y) * ballRigidbody.gravityScale;
        float t = Mathf.Sqrt(2 * (ballTransform.position.y - groundY) / gOfBall);//落地时间
        float placement = ballTransform.position.x + ballRigidbody.velocity.x * t;//此处将横向速度视为匀速
        return placement;
    }

    private float DistanceFromJumpArea()
    {
        //计算球距离纵跳击球区域距离
        float distance;
        if (ballTransform.position.x < jumpAreaFrontX)
            distance = ballTransform.position.x - jumpAreaFrontX;
        else if (ballTransform.position.x > jumpAreaAfterX)
            distance = ballTransform.position.x - jumpAreaAfterX;
        else
            distance = 0;
        return distance;
    }

    //区域计算
    private void UpdateHorizontalAreaData()//横向空间根据当前位置实时更新
    {
        //计算纵跳横向空间（以当前横坐标为基点）
        jumpAreaFrontX = transform.position.x - jumpAreaFrontOffset;
        jumpAreaAfterX = transform.position.x + jumpAreaAfterOffset;
        lineBetweenFrontAndMiddle = transform.position.x - lineBetweenFrontAndMiddleOffset;
        lineBetweenMiddleAndBack = transform.position.x + lineBetweenMiddleAndBackOffset;
    }

    //调用函数
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isJumping && collision.gameObject.tag == "Ground")
        {
            jumpEffect.SetActive(false);
            isJumping = false;
            animator.SetTrigger("GetToGround");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)//防止球与地面同时与player碰撞
    {
        if (isJumping && collision.gameObject.tag == "Ground")
        {
            jumpEffect.SetActive(false);
            isJumping = false;
            animator.SetTrigger("GetToGround");
        }
    }

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        g = Mathf.Abs(Physics2D.gravity.y) * rigidbody2d.gravityScale;
        InitializeState();
        //CalculateVerticalAreaData();
    }

    private void Update()
    {
        UpdateHorizontalAreaData();
        StateUpdate();
    }
}
