using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 파이어베이스 내장 메서드에서 유니티의 UI이 안되는 문제 해결을 위한 클레스
// 파이어베이스는 기본적으로 데이터들을 비동기로 읽기 때문에 UI가 함수들이 제대로 안 먹히는 부분(메인 쓰레드가 아니기때문)이 있습니다.
// 유니티 엔진에서는 메인 스레드가 아닌 다른 스레드에서 게임오브젝트, 트랜스폼 등 유니티 API에 접근할 수 없게 제한되어 있다.
// 그렇기때문에 메인쓰레드에서 UI를 처리를 해줘야합니다.
// 큐에 액션을 넣어서 메인 쓰레드에서 처리함.
public class UnityMainThreadDispatcher : MonoBehaviour
{
    public static UnityMainThreadDispatcher instance;
    
    private static readonly Queue<Action> executionQueue = new Queue<Action>();    // 읽기만 가능. readonly
    
    void Awake()
    {
        instance = this;
    }
    
    void FixedUpdate()
    {
        // 들어와 있는 큐가 있는지 계속 확인.(메인 스레드)
        // lock를 활용하여, 완료되기 전에, 접근하지 못하도록 함.
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void MethodEnqueue(Action methodToExecute)
    {
        // lock를 활용하여, 완료되기 전에, 접근하지 못하도록 함.
        lock (executionQueue)
        {
            executionQueue.Enqueue(methodToExecute);
        }
    }
    
    public void CoroutineEnqueue(IEnumerator action)
    {
        // lock를 활용하여, 완료되기 전에, 접근하지 못하도록 함.
        lock (executionQueue)
        {
            executionQueue.Enqueue(() => StartCoroutine(action));
        }
    }
    
    // public void Enqueue(Action action)
    // {
    //     CoroutineEnqueue(ActionWrapper(action));
    // }
    //
    // IEnumerator ActionWrapper(Action action)
    // {
    //     action();
    //     yield return null;
    // }
}
