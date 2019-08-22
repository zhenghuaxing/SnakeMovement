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
    float CONST_LENGTH = 1.5f;

    void Start()
    {
        snakeTouspeed = 0.5f;
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
        this.followHead(m_SnakeBodys, previousPositions);
    }

    void followHead(List<SnakeBody> tail, LinkedList<PathItem> path)
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

    void followHeadXX(List<SnakeBody> tail, LinkedList<Vector3> path)
    {
        // assuming at least one node in linked list
        var segmentStart = path.First.Value;
        var segmentEndNode = path.First.Next;

        var lengthToSegmentEnd = 0.0f;

        SnakeBody snakeBodyParent = this;
        foreach (var part in tail)
        {
            var segmentEnd = Vector3.zero;
            var segmentDiff = Vector3.zero;
            var segmentLength = 0f;
            var lengthToSegmentStart = lengthToSegmentEnd;

            // advance to correct segment if needed
            while (part.DistanceToParent(snakeBodyParent) > lengthToSegmentEnd)
            {
                if (segmentEndNode == null)
                {
                    // path too short
                    // NullReferenceException inbound, if not handled
                    break;
                }

                segmentEnd = segmentEndNode.Value;
                segmentDiff = segmentEnd - segmentStart;
                segmentLength = segmentDiff.magnitude;
                lengthToSegmentEnd += segmentLength;
                segmentStart = segmentEndNode.Value;
                segmentEndNode = segmentEndNode.Next;
            }

            // interpolate position on segment
            var distanceLeft = part.DistanceToParent(snakeBodyParent) - lengthToSegmentStart;
            var percentageAlongSegment = distanceLeft / segmentLength;

            part.transform.position = segmentStart +
                            segmentDiff * percentageAlongSegment;

            segmentStart = segmentEnd;
            snakeBodyParent = part;
        }

        // cutting off unnecessary end of path
        while (segmentEndNode != path.Last)
        {
            path.RemoveLast();
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
