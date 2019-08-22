using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PathItem{
    public Vector3 position;
    public float distance;
}

public class SnakeHeader : SnakeBody 
{
    public Camera cam;
    public GameObject SnakeBodyPrefab;
    public GameObject snakeBodyMgr;

    [Header("Private")]
    private Vector3 MouseVe;
    private Vector3 mous;
    private float x;
    private float snakeTouspeed;

    // 记录位置信息
    LinkedList<PathItem> previousPositions = new LinkedList<PathItem>();
    // 记录蛇身节点
    List<SnakeBody> m_SnakeBodys = new List<SnakeBody>();
    // 固定的距离；
    float CONST_LENGTH = 1.0f;

    void Start()
    {
        snakeTouspeed = 3.5f;
        this.RecordPosition(transform.position, true);
        // 初始化加载
        for(int i = 0; i<30; ++i){
            this.AppendSnakeBody();
        }
    }

    // Update is called once per frame
    void Update()
    {
        moussMove();
        // update movement
        transform.Translate(Vector3.forward.normalized *GameInfo._instance.playSpeed, Space.World); // 头节点的位置
        this.RecordPosition(transform.position);
        this.FollowHead(m_SnakeBodys, previousPositions);

        if(Input.GetKeyDown(KeyCode.Space)){
            this.DestroyFirstBody();
        }
    }

    void FollowHead(List<SnakeBody> tail, LinkedList<PathItem> path)
    {
        // 包含父亲节点的路径的起点
        LinkedListNode<PathItem> startNode = path.First;
        // 父节点距离其路径片段的尾部的距离
        float distanceToEnd = startNode.Value.distance;
        foreach (SnakeBody body in tail){
            //////// 计算 body 位置 ////////
            if(startNode.Next == null){
                // 已经是尾巴了
                break;
            }

            // 距离不足，无法设置位置
            while(distanceToEnd < CONST_LENGTH){
                if(startNode.Next.Next == null){
                    return;
                }
                startNode = startNode.Next;
                distanceToEnd = distanceToEnd + startNode.Value.distance;
            }

            if(distanceToEnd >= CONST_LENGTH){
                // body 应该在片段 startNode 开头的线段上
                var ds = distanceToEnd - CONST_LENGTH;
                var rate = 1.0f - ds/startNode.Value.distance;
                var pos = Vector3.LerpUnclamped(startNode.Value.position, startNode.Next.Value.position, rate);
                body.transform.position = pos;
                distanceToEnd = distanceToEnd - CONST_LENGTH;
            }
        }

        while (startNode.Next != path.Last)
        {
            path.RemoveLast();
            path.Last.Value.distance = 0;
        }
    }

    LinkedListNode<PathItem> RecordPosition(Vector3 pos, bool first = false){
        if(first){
            var item = new PathItem();
            item.position = pos;
            item.distance = 0;
            return this.previousPositions.AddFirst(item);
        }else{
            var preItem = this.previousPositions.First.Value;
            var item = new PathItem();
            item.position = pos;
            item.distance = Vector3.Distance(preItem.position, item.position);
            return this.previousPositions.AddFirst(item);
        }
    }

    // 添加一个蛇身
    void AppendSnakeBody(){
        // 放在尾部节点位置放蛇的 body
        var lastPos = this.GetLastTailPosition();
        var gameObj = Instantiate(this.SnakeBodyPrefab);
        gameObj.transform.parent = this.snakeBodyMgr.transform;
        gameObj.transform.position = lastPos;
        gameObj.transform.rotation = transform.rotation;
        var body = gameObj.GetComponent<SnakeBody>();
        this.m_SnakeBodys.Add(body);
    }

    // 删除某个节点
    void DestroyFirstBody(){
        if(this.m_SnakeBodys.Count > 0){
            var bodyToRemove = this.m_SnakeBodys[0];
            this.m_SnakeBodys.RemoveAt(0);
            Destroy(bodyToRemove.gameObject);
        }
    }

    // 获取最后一个节点的位置，作为新的节点的初始位置
    Vector3 GetLastTailPosition(){
        if(m_SnakeBodys.Count == 0){
            return this.transform.position;
        }else{
            return m_SnakeBodys[m_SnakeBodys.Count-1].transform.position;
        }
    }

    void moussMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseVe = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            mous = cam.ScreenToWorldPoint(Input.mousePosition);
            x = mous.x - MouseVe.x;
            transform.position = new Vector3(transform.position.x + x * snakeTouspeed, transform.position.y, transform.position.z);
            MouseVe = cam.ScreenToWorldPoint(Input.mousePosition);

            if (transform.position.x >= 8f)
            {
                transform.position = new Vector3(8, transform.position.y, transform.position.z);
            }
            if (transform.position.x <= -8f)
            {
                transform.position = new Vector3(-8, transform.position.y, transform.position.z);
            }
        }
    }
}
