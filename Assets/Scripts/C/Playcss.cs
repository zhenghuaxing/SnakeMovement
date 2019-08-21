using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// namespace stl
// {
//     class DLinkedNode<T>{
//         public T data;
//         public DLinkedNode<T> next = null;
//         public DLinkedNode<T> prev = null;

//         public DLinkedNode(){}
//         public DLinkedNode(T value) { data = value; }
//     }
// }

public class Playcss : MonoBehaviour
{
    [Header("鼠标点击位置")]
    private Vector3 MouseVe;
    public Camera cam;
    public float x;
    public Vector3 mous;
    private float snakeTouspeed;

    [SerializeField]
    private List<Transform> BodyTran = new List<Transform>();
    private Vector3 headPos;
    private float DISTANCE = 1.0f;

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
        while(this.DLCount > 100){
            this.RemoveLast();
        }
    }

    void Start()
    {
        snakeTouspeed = 2.5f;
        // 记录蛇头位置
        this.headPos = transform.position;
        // 更新蛇身位置
        int i=0;
        BodyTran.ForEach(bodyTran=>{
            bodyTran.position = new Vector3(this.headPos.x, this.headPos.y, this.headPos.z - (i+1)*DISTANCE);
            ++i;
        });
        
        // 初始化记录所有位置数据
        this.InitDLink();
        this.AppendDLNode(this.headPos);
        BodyTran.ForEach(bodyTran=>this.AppendDLNode(bodyTran.position));
    }

    private void Update()
    {
        moussMove();
        UpdateNodesPosition();
    }

    private void UpdateNodesPosition()
    {
        transform.Translate(Vector3.forward.normalized *GameInfo._instance.playSpeed, Space.World);
        this.headPos = transform.position;
        this.PrependDLNode(this.headPos);
        this.ReduceDLSize();

        // 确定每一个子节点的位置
        // 上一个节点现在被摆放的位置
        stl.DLinkedNode<Vector3> preNode = this.dlHead.next;
        for(var index = 0; index < BodyTran.Count; ++index){
            Debug.Log($" Start >>>>index:{index}  x:{preNode.data.x}  z:{preNode.data.z} ");
            // 根据上一个节点 preNode 确定当前节点的位置 
            // 基准节点
            var smallNode = preNode;
            var bigNode = preNode.next;
            var tmpNode = bigNode;
            Debug.Log("is tmpNode last:" + (tmpNode == this.dlTail));
            Debug.Log("is tmpNode.next last:" + (tmpNode.next == this.dlTail));
            while(tmpNode != this.dlTail){
                var pos = tmpNode.data;
                var disPow = DistanceEx(pos, preNode.data);
                var ds = disPow-DISTANCE*DISTANCE;
                if(ds < -0.05f){
                    smallNode = tmpNode;
                    tmpNode = tmpNode.next;
                    if(tmpNode == this.dlTail){
                        // 最后一个节点；直接使用 smallNode
                        BodyTran[index].position = pos;
                        preNode = smallNode;
                        break;
                    }
                }else if( ds < 0.05f){
                    // 如果 pos 和 smallNode 的距离近似等于 1,
                    BodyTran[index].position = pos;
                    preNode = tmpNode;
                    break;
                }else{
                    bigNode = tmpNode;
                    // small 和 big 之间做近似计算
                    float rate = Mathf.Sqrt(disPow) / DISTANCE;
                    float _x = Mathf.Lerp(smallNode.data.x, bigNode.data.x, rate);
                    float _z = Mathf.Lerp(smallNode.data.z, bigNode.data.z, rate);
                    var newPos = new Vector3(_x, BodyTran[index].position.y, _z);
                    BodyTran[index].position = newPos;
                    // 在 small 后插入新的位置节点并更新 preNode
                    preNode = this.AppendDLNode(newPos, smallNode);
                    // Debug.Log($">>>>index:{index}  x:{preNode.data.x}  z:{preNode.data.z} ");
                    break;
                }
            }
        }
    }

    float DistanceEx(Vector3 v1, Vector3 v2)
    {
        float dx = v1.x - v2.x;
        float dz = v1.z - v2.z;
        float s2 = dx * dx + dz * dz;
        return s2;
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

}
