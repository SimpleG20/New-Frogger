using Cysharp.Threading.Tasks;

public abstract class BaseServiceAsync<Tout, Targ>
{
    public abstract UniTask<Tout> call(Targ arg);
}

public class NoArg {}