using UnityEngine;

[CreateAssetMenu]
public class EnemyWave : ScriptableObject
{

    [System.Serializable]
    public struct State
    {
        EnemyWave wave;
        int index;
        EnemySpawnSequence.State sequence;

        public State(EnemyWave wave)
        {
            this.wave = wave;
            index = 0;
            Debug.Assert(wave.spawnSequences.Length > 0, "Empty wave!");
            sequence = wave.spawnSequences[0].Begin();
        }

        public float Progress(float deltaTime)
        {
            deltaTime = sequence.Progress(deltaTime);
            while (deltaTime >= 0f)
            {
                if (++index >= wave.spawnSequences.Length)
                {
                    return deltaTime;
                }
                sequence = wave.spawnSequences[index].Begin();
                deltaTime = sequence.Progress(deltaTime);
            }
            return -1f;
        }
    }

    [SerializeField]
    private EnemySpawnSequence[] spawnSequences = {
        new EnemySpawnSequence()
    };

    public State Begin() => new State(this);
}