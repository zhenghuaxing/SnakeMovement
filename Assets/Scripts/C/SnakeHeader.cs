using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stl
{
    public class DLinkedNode<T>{
        public T data;
        public DLinkedNode<T> next = null;
        public DLinkedNode<T> prev = null;

        public DLinkedNode(){}
        public DLinkedNode(T value) { data = value; }
    }
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

    // Start is called before the first frame update
    void Start()
    {
        this.InitDLink();
        snakeTouspeed = 2.5f;
        // 初始化记录头节点的位置信息(最新的轨迹点放在最前面)
        this.PrependDLNode(transform.position);
        // 初始化加载
        for(int i = 0; i<50; ++i){
            this.AppendSnakeBody();
        }
    }

    // Update is called once per frame
    void Update()
    {
        moussMove();
        transform.Translate(Vector3.forward.normalized *GameInfo._instance.playSpeed, Space.World); // 头节点的位置
        // 最新的数据
        var node = this.PrependDLNode(transform.position);
        this.ReduceDLSize();
        this.UpdateNext(node, 0);
        // this.DumpLink();
    }

    void AppendSnakeBody(){
        var pos = this.GetLastPosition();
        var _x = pos.x;
        var _y = transform.position.y;
        var _z = pos.z - DISTANCE;
        var gameObj = Instantiate(this.SnakeBodyPrefab);
        gameObj.transform.parent = this.snakeBodyMgr.transform;
        gameObj.transform.position = new Vector3(_x, _y, _z);
        gameObj.transform.rotation = transform.rotation;
        var body = gameObj.GetComponent<SnakeBody>();
        this.Append(body);
        // 新增加在尾部的点，其位置数据变成最旧的数据
        this.AppendDLNode(body.transform.position);
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
            //if (x>0)
            //{
            //    transform.Translate(Vector3.right* snakeTouspeed);
            //}
            //else if(x < 0)
            //{
            //    transform.Translate(Vector3.left * snakeTouspeed);
            //}

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

            //mous = cam.ScreenToWorldPoint(Input.mousePosition);
        }

    }

    // 最新的数据放在最后面
    [Header("DLinkedList")]
    private stl.DLinkedNode<Vector3> dlHead = new stl.DLinkedNode<Vector3>();
    private stl.DLinkedNode<Vector3> dlTail = new stl.DLinkedNode<Vector3>();
    public int DLCount = 0;
    void InitDLink(){
        this.dlHead.next = this.dlTail;
        this.dlHead.prev = null;
        this.dlTail.prev = this.dlHead;
        this.dlTail.next = null;
        this.DLCount = 0;
    }
    bool IsDLEmpty(){ return DLCount == 0; }
    stl.DLinkedNode<Vector3> AppendDLNode(Vector3 data){
        return this.AppendDLNode(data, this.dlTail.prev);
    }
    stl.DLinkedNode<Vector3> PrependDLNode(Vector3 data){
        return this.AppendDLNode(data, this.dlHead);
    }

    stl.DLinkedNode<Vector3> AppendDLNode(Vector3 data, stl.DLinkedNode<Vector3> target){
        var node = new stl.DLinkedNode<Vector3>(data);
        target.next.prev = node;
        node.next = target.next;
        node.prev = target;
        target.next = node;
        ++this.DLCount;
        return node;
    }

    stl.DLinkedNode<Vector3> RemoveLast(){
        if(this.DLCount == 0){
            return null;
        }
        var node = this.dlTail.prev;
        node.prev.next = node.next;
        node.next.prev = node.prev;
        node.next = null;
        node.prev = null;
        --this.DLCount;
        return node;
    }


    private void ReduceDLSize(){
        while(this.DLCount > 60){
            this.RemoveLast();
        }
    }

    private void DumpLink(){
        var cur = this.dlHead.next;
        Debug.Log(">>>>>>>>>>>>>>>>>>>>>>> start ");
        while(cur != this.dlTail){
            var data = cur.data;
            Debug.Log($">>x:{data.x} y:{data.y} z:{data.z}");
            cur = cur.next;
        }
        Debug.Log(">>>>>>>>>>>>>>>>>>>>>>> end ");
    }
}
