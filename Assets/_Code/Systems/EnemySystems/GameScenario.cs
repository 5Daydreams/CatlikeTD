using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject
{

    [System.Serializable]
    public struct State
    {
        GameScenario scenario;

        int index;

        EnemyWave.State wave;

        public State(GameScenario scenario)
        {
            this.scenario = scenario;
            index = 0;
            Debug.Assert(scenario.waves.Length > 0, "Empty scenario!");
            wave = scenario.waves[0].Begin();
        }

        public bool Progress()
        {
            float deltaTime = wave.Progress(Time.deltaTime);
            while (deltaTime >= 0f)
            {
                if (++index >= scenario.waves.Length)
                {
                    return false; // progress is over
                }
                wave = scenario.waves[index].Begin();
                deltaTime = wave.Progress(deltaTime);
            }
            return true; // progress is not yet done
        }
    }

    [SerializeField] private EnemyWave[] waves = { };

    public State Begin() => new State(this);
}