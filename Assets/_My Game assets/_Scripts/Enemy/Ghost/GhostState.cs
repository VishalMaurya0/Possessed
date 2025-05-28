public abstract class GhostState
{
    protected GhostAI ghostAI;

    public GhostState(GhostAI ghostAI)
    {
        this.ghostAI = ghostAI;
    }

    
    public virtual void EnterState() { }
    public virtual void UpdateState() { }
    public virtual void ExitState() { }
}
