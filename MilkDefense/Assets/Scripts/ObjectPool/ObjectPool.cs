using System.Collections.Generic;


//  > 오브젝트 풀링을 수행하는 클래스입니다.
/// - T : 인터페이스 IObjectPoolable 을 구현하는 클래스만 사용될 수 있습니다.
/// - 사용 방법 : 인터페이스 IObjectPoolable 을 구현하는 인스턴스에서 canRecyclable 의 
///   값을 false 로 변경하는 경우 재사용 타깃이 됩니다.
public class ObjectPool<T> where T : class, IObjectPoolable
{
    //  > 풀링할 오브젝트들을 참조할 리스트
    private List<T> _PoolObject = new List<T>();

    //  > 재활용할 오브젝트가 존재하는지 검사합니다.
    /// - return : 재활용 가능한 오브젝트가 존재할 경우 true 를 반환합니다.
    public bool canRecycle => (_PoolObject.Find((T poolableObject) => poolableObject.canRecyclable) != null);

    //  > 풀링할 새로운 오브젝트를 등록합니다.
    /// - return : 등록한 객체 (newRecyclableObject) 를 그대로 리턴합니다.
    public T RegisterRecyclableObject(T newRecyclableObject)
    {
        _PoolObject.Add(newRecyclableObject);
        return newRecyclableObject;
    }

    //  > 오브젝트 풀에 등록한 객체를 등록 해제합니다.
    public void UnRegisterRecyclableObject(params T[] recyclableObject)
    {
        foreach (var recyclable in recyclableObject)
            _PoolObject.Remove(recyclable);
    }

    //  > 재활용된 오브젝트를 얻습니다.
    /// - checkCanRecycle : true 일 경우 재사용 가능한 오브젝트 존재 여부를 검사합니다.
    /// - return : 재활용된 오브젝트를 리턴합니다.
    public T GetRecycledObject(bool checkCanRecycle = false)
    {
        //  > 재사용 가능한 오브젝트가 존재하는지 검사합니다.
        if (checkCanRecycle) if (!canRecycle) return null;

        //  > 재사용 가능한 오브젝트를 찾습니다.
        T recyclableObject = _PoolObject.Find((T poolableObject) => poolableObject.canRecyclable);

        //  > OnRecycleStartSignature 를 호출합니다.
        recyclableObject.OnRecycleStartSignature?.Invoke();

        //  > 재사용됨 상태로 변경합니다.
        recyclableObject.canRecyclable = true;

        //  > OnRecycleFinishSignature 를 호출합니다.
        recyclableObject.OnRecycleFinishSignature?.Invoke();

        //  > 재사용된 오브젝트를 리턴합니다.
        return recyclableObject;
    }
}