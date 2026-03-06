# 🏫 SchoolSim - Utility AI School Simulation

[English]
A school life simulation game where 20 NPC students autonomously choose actions based on their dynamic needs (Hunger, Fatigue, Social, etc.) using a **Utility AI** decision-making system. Built with Unity and C#.

[日本語]
30人のNPC生徒が、自身の欲求（空腹、疲労、社交など）の変化に応じて自律的に行動を選択するシミュレーションゲームです。**Utility AI**アーキテクチャを採用し、UnityとC#で開発しました。

---

## Demo Video
[youtube](https://youtu.be/p1hnskDwTvM)


---

## Core Systems & Architecture (コアシステムと設計)

### 1. Utility AI Decision Pipeline (意思決定パイプライン)
NPCs evaluate every possible action in the world and choose the best one. The calculation is processed in the following pipeline:
NPCはワールド内のすべての可能なアクションを評価し、最適なものを選択します。

1. **Calculate Side Effects (副作用の計算):** Predicts how an action will change the NPC's stats or relationships.
2. **Apply Need Modifiers (欲求ごとの重み付け):** Modifies the base score using mathematical curves based on the need type:
   - *Existence Needs (生存欲求):* `Mathf.Pow(0.3f, -(diff - 50) / 3)`
   - *Relation Needs (関係欲求):* `diff * diff * diff`
   - *Growth Needs (成長欲求):* `2000f * (diff) + 1000`
3. **Distance Penalty (距離ペナルティ):** Divides the score by a distance penalty (starts at 1.0, +0.1 per cell).
4. **Weighted Random Selection (重み付きランダム選択):** Filters the top 6 actions and picks one using a weighted random selection to ensure natural, non-mechanical behavior.

### 2. Performance Optimization: Zero GC Spikes (GCスパイク対策)
To prevent Garbage Collection (GC) spikes during heavy Utility AI calculations across multiple NPCs, I optimized the memory allocation:
多数のNPCが同時にAI計算を行う際のGCスパイクを防ぐため、メモリ割り当てを最適化しました。

- Used `struct DeltaResult` instead of classes.
- Passed parameters by reference (`ref`) in `CalcScore()` to avoid re-initializing Dictionaries for character relations every frame.

```csharp
    public float CalcScore(CharacterStats deltas, IInteractable o, ref DeltaResult result)
    {
        float eMod = ConfigData.Instance.eModifier;
        float rMod = ConfigData.Instance.rModifier;
        float gMod = ConfigData.Instance.gModifier;

        var i_e_mod = ConfigData.Instance.I_E_modifier;
        var n_s_mod = ConfigData.Instance.N_S_modifier;

        // Apply MBTI personality modifiers to Needs
        if (character.Data.mbti.CheckComponent(MBTIComponent.S))
        {
            eMod *= n_s_mod; 
        }
        else if (character.Data.mbti.CheckComponent(MBTIComponent.N))
        {
            gMod *= n_s_mod;
        }

        if (character.Data.mbti.CheckComponent(MBTIComponent.E))
        {
            rMod *= i_e_mod;
        }
        else
        {
            rMod /= i_e_mod;
        }
        
        var score =
            (deltas *
                (CalcENeedScoreMultiplier(character.Data.stats) * eMod +
                CalcRNeedScoreMultiplier(character.Data.stats) * rMod +
                CalcGNeedScoreMultiplier(character.Data.stats) * gMod)).SumNeeds();
        
        var dist = NavManager.Instance.FindPathAround(character.Position, o?.Positions).Item2;
        float distancePenalty = 1f + (dist * 0.1f); 
        // distはgrid上の距離です。

        return score / distancePenalty;
    }


    private float CalcENeedScoreMultiplier(float val)
    {
        var diff = (100 - val > 0) ? 100 - val : 0;
        return Mathf.Pow(0.3f, -(diff - 50) / 3);
    }
    private float CalcRNeedScoreMultiplier(float val)
    {
        var diff = (100 - val > 0) ? 100 - val : 0;
        return diff * diff * diff;
    }

    private float CalcGNeedScoreMultiplier(float val)
    {
        var diff = (100 - val > 0) ? 100 - val : 0;
        return 2000f * (diff) + 1000;
    }
```

## The impact of MBTI

To make agents behave more like real humans, I integrated an **MBTI-based personality system**. This system modifies the base multipliers of the Utility AI, creating distinct behavioral patterns without hardcoding separate logic for each personality.
エージェントに人間らしい多様性を持たせるため、**MBTIベースの性格システム**を導入しました。これにより、AIの個別ロジックを分岐させることなく、欲求の優先度計算に自然な個性を生み出しています。

| MBTI Component | Meaning (属性) | Impact on Utility AI (AIへの影響) |
|:---:|:---|:---|
| **E** | Extraverted (外向型) | **Relation Needs ↑**<br>Actively seeks social interactions. (社交・人間関係を強く優先する) |
| **I** | Introverted (内向型) | **Relation Needs ↓**<br>Comfortable being alone, less social urgency. (一人の時間を好み、社交欲求の優先度が下がる) |
| **S** | Sensing (感覚型) | **Existence Needs ↑**<br>Prioritizes immediate physical needs like Food/Rest. (食事や睡眠など、現在の身体的・生存的な欲求を優先する) |
| **N** | Intuition (直観型) | **Growth Needs ↑**<br>Prioritizes learning, curiosity, and self-improvement. (学習や好奇心など、成長に関する欲求を優先する) |
| **T/F , J/P** | Others | *In Development (拡張予定)* |

By combining these modifiers (`n_s_mod`, `i_e_mod`), the exact same environment produces entirely different outcomes. For example, an **EN** student will prioritize socializing and learning, while an **IS** student will prioritize resting alone or fulfilling immediate physical needs.
これらの補正値（モディファイア）を組み合わせることで、同じ状況下でも**「EN型の生徒は友達と話し、IS型の生徒は一人で休む」**といった創発的なドラマが自然に発生します。

```csharp
    if (character.Data.mbti.CheckComponent(MBTIComponent.S))
    {
        // MBTI S
        eMod *= n_s_mod; // more weight for existance needs
    }
    else if (character.Data.mbti.CheckComponent(MBTIComponent.N))
    {
        // MBTI N
        gMod *= n_s_mod; // more weight for growth needs
    }

    if (character.Data.mbti.CheckComponent(MBTIComponent.E))
    {
        // MBTI E
        rMod *= i_e_mod; // more weight for relation needs
    }
    else
    {
        // MBTI I
        rMod /= i_e_mod; // less weight for relation needs
    }
```

## Proof with Plotting Tool

Balancing complex AI needs is chaotic. I developed custom Python tools to visualize the AI's behavior, and verify the stability of the simulation objectively.

|  File Name  |                            Explain                           |
|-------------|--------------------------------------------------------------|
| NeedsMSD.py | Plots the Mean Squared Deviation (MSD) to analyze the stability of needs. |
| PersonalNeeds.py | Visualizes changes in individual NPC needs over time.                    |
| PlottingTypeScoreFunction.py | Graphs the non-linear urgency functions for parameter tuning. |
| ShowDisutilty.py | Shows the average urgency ratio of needs by type. |
| Strikezone.py | Simulates the "Strike Zone" mechanic for the interpersonal attraction system. |
| StrikeZoneImpact.py | Analyzes and proves the impact of the Strike Zone system. |

## Current Phase
I am currently addressing "Priority Inversion" edge cases (e.g., an NPC prioritizing a social chat over severe hunger). I am actively using my Python visualization tools to fine-tune the parameter "sweet spot" to ensure both logical survival and emergent human-like drama.
（現在、極度の空腹時でも会話を優先してしまうような「優先度逆転」の課題に取り組んでいます。自作のPython分析ツールを活用し、生存のための論理的行動と、人間らしいドラマが両立するパラメータの「スイートスポット」をデータ駆動で調整中です。）