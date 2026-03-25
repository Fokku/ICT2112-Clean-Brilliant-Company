public abstract class EmissionSource
{
    protected float emission;
    protected string name;

    public string GetName()
    {
        return name;
    }

    public float getEmission()
    {
        return emission;
    }

    public void setEmission(float emission)
    {
        this.emission = emission;
    }
}