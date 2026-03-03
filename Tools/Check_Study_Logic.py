import json
import numpy as np
import plotly.graph_objects as go
from collections import defaultdict

def analyze_motivation_anomaly(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    ch_datas = data.get('chDatas', [])
    char_history = defaultdict(list)

    # 성적에 포함될 과목 리스트
    subject_keys = ['attractive', 'conversation', 'comedy', 'literature', 
                    'math', 'sociology', 'science', 'sports', 'art']

    for entry in ch_datas:
        char_id = entry['id']
        # 성적 합산 로직 (직접 필드에 있거나 stats 안에 있는 경우 모두 대응)
        total_score = 0
        stats = entry.get('stats', {})
        for key in subject_keys:
            # entry 바로 아래 있거나, stats 딕셔너리 안에 있는 값을 합산
            val = entry.get(key) if entry.get(key) is not None else stats.get(key, 0)
            total_score += val

        char_history[char_id].append({
            'tick': entry['tick'],
            'name': entry['charName'],
            'score': total_score,
            'motivation': stats.get('motivation', 0)
        })

    fig = go.Figure()

    for char_id, history in char_history.items():
        history.sort(key=lambda x: x['tick'])
        ticks = [h['tick'] for h in history]
        scores = [h['score'] for h in history]
        motivations = [h['motivation'] for h in history]
        name = history[0]['name']

        # 기울기 계산
        score_gradient = np.gradient(scores) if len(scores) > 1 else np.zeros(len(scores))
        mot_gradient = np.gradient(motivations) if len(motivations) > 1 else np.zeros(len(motivations))
        
        anomalies_x = []
        anomalies_y = []
        
        for i in range(len(score_gradient)):
            # 조건: 성적 변화는 거의 없는데(정체), 모티베이션 변화량은 클 때(요동침)
            if abs(score_gradient[i]) < 0.1 and abs(mot_gradient[i]) > 1.0:
                anomalies_x.append(ticks[i])
                anomalies_y.append(motivations[i])

        # 모티베이션 기본 선
        fig.add_trace(go.Scatter(
            x=ticks, y=motivations,
            mode='lines',
            name=f"{name} (Mot)",
            hovertemplate=f"<b>{name}</b><br>Tick: %{{x}}<br>Mot: %{{y:.1f}}<br>Total Score: %{{text:.1f}}<extra></extra>",
            text=scores
        ))

        # 이상 징후 마커 (X 표시)
        if anomalies_x:
            fig.add_trace(go.Scatter(
                x=anomalies_x, y=anomalies_y,
                mode='markers',
                name=f"{name} Anomaly",
                marker=dict(color='red', size=10, symbol='x-thin', line=dict(width=2)),
                hoverinfo='skip'
            ))

    fig.update_layout(
        title="<b>Motivation-Score Growth Mismatch Detection</b><br><sup>Red 'X' indicates Score Stagnation with High Motivation Volatility</sup>",
        xaxis_title="Tick (Time)",
        yaxis_title="Motivation Value",
        hovermode="x unified",
        template="plotly_white",
        legend=dict(orientation="h", yanchor="bottom", y=1.02, xanchor="right", x=1)
    )
    
    fig.show()

if __name__ == "__main__":
    plot_path = '../Assets/Log/log.json' # 경로 확인 필요
    analyze_motivation_anomaly(plot_path)