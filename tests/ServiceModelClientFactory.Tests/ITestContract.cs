using System.ServiceModel;

[ServiceContract]
public interface ITestContract

{
    [OperationContract]
    void TestOperation();
}
