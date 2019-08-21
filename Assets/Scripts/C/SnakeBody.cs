using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBody : MonoBehaviour
{
    protected float DISTANCE = 1.0f;
    public SnakeBody next = null; // 下一个节点

    // 已经在 node 数据是起点，调用此方法更新子节点的位置数据
    protected void UpdateNext(stl.DLinkedNode<Vector3> node, float costDis){
        if(next == null){ 
            return; 
        }
        stl.DLinkedNode<Vector3> start = node;
        stl.DLinkedNode<Vector3> end = start.next;
        float _costDis = costDis;
        while(true){
            if(end == null){
                Debug.Log("ERROR");
            }else{
                var dis = Vector3.Distance(start.data, end.data);
                if(Mathf.Abs(dis + _costDis - DISTANCE) < 0.08){
                    // 误差允许范围 end 节点就是目标节点
                    next.transform.position = end.data;
                    next.UpdateNext(end, 0);
                    break;
                }
                if(dis + _costDis > DISTANCE){
                    // 在线段上
                    var rate = (DISTANCE - _costDis)/dis;
                    Debug.Log($"rate: {rate}");
                    var pos = Vector3.LerpUnclamped(start.data, end.data, rate);
                    next.transform.position = pos;
                    // UI 节点迭代
                    var cost = dis-(DISTANCE-_costDis);
                    Debug.Log($">>>>迭代子节点 cost:{cost}");
                    next.UpdateNext(start, dis-(DISTANCE-_costDis));
                    break;
                }else{
                    // 迭代下一个数据节点
                    start = start.next;
                    end = start.next;
                    _costDis = _costDis + dis;
                }
            }
        }
    }

    protected Vector3 GetLastPosition(){
        if(next == null){
            return transform.position; 
        }else{
            return next.GetLastPosition();
        }
    }

    protected void Append(SnakeBody body){
        if(next == null){
            next = body;
        }else{
            next.Append(body);
        }
    }

    protected bool IsLast(){
        return this.next == null;
    }
}
