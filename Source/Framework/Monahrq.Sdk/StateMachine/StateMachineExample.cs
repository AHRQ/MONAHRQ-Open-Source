//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Windows;
//using Microsoft.Practices.Prism.Events;
//using Monahrq.Infrastructure.Domain.Reports;
//using Monahrq.Infrastructure.Services;

namespace Monahrq.Sdk.StateMachine
{
    //public class StateMachineRunner
    //{
    //    public StateMachineRunner()
    //    {
    //        var stateMachine = new StateMachineExample<Report>((e) =>
    //            {
    //               // EventAggregator.Publish<Error>(e);
    //            });

    //        stateMachine.Init();
    //        stateMachine.GotError();//go error
    //    }
    //}

    //public class DataService : DataServiceBase
    //{

    //}
    //public class StateMachineExample<T> : StateMachine
    //{
    //    [Trigger]
    //    public readonly Action Rollback;

    //    [Trigger]
    //    public readonly Action Update;

    //    [Trigger]
    //    public readonly Action Delete;

    //    [Trigger]
    //    public readonly Action Init;


    //    [Trigger]
    //    public readonly Action GotError;

    //    DataService DataService = new DataService();

    //    public StateMachineExample(Action<Exception> exceptionCallback)
    //        : base(exceptionCallback)
    //    {
    //        Init();
    //    }


    //    /*
    //     * 
    //     * 
    //     */

    //    private StateAction _create = new StateAction();
    //    private StateAction _update = new StateAction();
    //    private StateAction _delete = new StateAction();
    //    private StateAction _load = new StateAction();

        
    //    private int ID { get; set; }
    //    T Enity { get; set; }
    //    private int _hashCode { get { return this.GetHashCode(); } }



    //    initialized:
    //        Debug.Print("Object id {0} initialized", _hashCode);
    //        yield return null;
    //        if (Trigger == Init) goto loaded;
    //        InvalidTrigger();

    //    loaded:
    //        Debug.Print("Object id {0} entered loaded state", _hashCode);
           
    //        _load.Execute = () => DataService.GetEntityById<T>(ID, (result, err) =>
    //            {
    //                exception = err;
    //                Enity = result;
    //                _load.Invoked();
    //            });

    //        yield return null;

    //        if (Trigger == Validate) goto validating;
    //        if (Trigger == Delete) goto delete;

    //        InvalidTrigger();

    //    validating:
    //        Debug.Print("Object id {0} entered validating state", _hashCode);
    //        yield return null;
            
    //        if (Trigger == GotError) goto error;
    //        if (Trigger == Rollback) goto loaded;
    //        if (Trigger==Update) goto isvalid;

    //        InvalidTrigger();

    //    delete:
    //        Debug.Print("Object id {0} entered delete state", _hashCode);
    //        yield return null;

    //        if (Trigger == GotError) goto error;
    //        InvalidTrigger();

    //    isvalid:
    //        Debug.Print("Object id {0} entered saving state", _hashCode);
            
    //        yield return null;
    //        if(Trigger==Update) goto issaving;
    //        if(Trigger==Delete) goto  delete;
    //        InvalidTrigger();

    //    issaving:
    //        Debug.Print("Object id {0} entered saving state", _hashCode);
    //        yield return null;
    //        if (Trigger == GotError) goto error;
    //        InvalidTrigger();

    //    iscommited:
    //        Debug.Print("Object id {0} entered commited", _hashCode);
    //        yield return null;
    //        if (Trigger == GotError) goto error;
    //        InvalidTrigger();

    //    error:
    //        Debug.Print("Object id {0} entered error state", _hashCode);
    //        yield return null;

    //        if (Trigger == Rollback) goto loaded;
    //        InvalidTrigger();
    //    }
    //}
}
