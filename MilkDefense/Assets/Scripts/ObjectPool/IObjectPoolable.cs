using System;

//  > 오브젝트 풀링에 사용되는 컴포넌트에서 구현해야 하는 인터페이스입니다.
public interface IObjectPoolable
{
    //  > 오브젝트가 재사용될 수 있음을 나타내는 프로퍼티입니다.
    bool canRecyclable { get; set; }

    //  > 오브젝트가 재사용되기 전에 호출되는 대리자입니다.
    Action OnRecycleStartSignature { get; }

    //  > 오브젝트가 재사용된 후에 호출되는 대리자입니다.
    Action OnRecycleFinishSignature { get; }
}