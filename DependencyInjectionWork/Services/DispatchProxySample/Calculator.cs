namespace DependencyInjectionWork.Services.DispatchProxySample
{
    public class Calculator : ICalculator
    {
        public Calculator(IService service)
        {

        }

        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
