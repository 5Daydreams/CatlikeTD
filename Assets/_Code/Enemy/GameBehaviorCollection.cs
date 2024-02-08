using System.Collections.Generic;

[System.Serializable]
public class GameBehaviorCollection
{
    private List<GameBehavior> behaviours = new List<GameBehavior>();
    public bool IsEmpty => behaviours.Count == 0;

    public void Add(GameBehavior behaviour)
    {
        behaviours.Add(behaviour);
    }

    public void Clear()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            behaviours[i].Recycle();
        }
        behaviours.Clear();
    }

    public void GameUpdate()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            if (!behaviours[i].GameUpdate())
            {
                int lastIndex = behaviours.Count - 1;
                behaviours[i] = behaviours[lastIndex];
                behaviours.RemoveAt(lastIndex);
                i -= 1;
            }
        }
    }
}