import json
import plotly.graph_objects as go
from collections import defaultdict
from common import *

def plot_total_disutility_plotly(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # 욕구 분류
    e_needs = ['hungry', 'fatigue', 'toilet', 'hygiene']
    r_needs = ['fun', 'loneliness', 'rLoneliness']
    g_needs = ['motivation']
    
    char_data = defaultdict(lambda: defaultdict(float))
    
    # 정규화를 위한 최대값 미리 계산
    max_e_need_multiplier = calc_e_need_multiplier(0)
    max_r_need_multiplier = calc_r_need_multiplier(0)
    max_g_need_multiplier = calc_g_need_multiplier(0)

    for entry in data.get('chDatas', []):
        tick = entry['tick']
        char_name = entry['charName']
        stats = entry.get('stats', {})
        
        total_disutility = 0.0

        # 1. 생존 / 2. 관계 / 3. 성장 욕구 합산 및 정규화
        for key in e_needs:
            if key in stats:
                total_disutility += (calc_e_need_multiplier(stats[key]) / max_e_need_multiplier)
        for key in r_needs:
            if key in stats:
                total_disutility += (calc_r_need_multiplier(stats[key]) / max_r_need_multiplier)
        for key in g_needs:
            if key in stats:
                total_disutility += (calc_g_need_multiplier(stats[key]) / max_g_need_multiplier)
                
        char_data[char_name][tick] = total_disutility / 3

    # Plotly Figure 생성
    fig = go.Figure()

    for char_name, ticks_data in char_data.items():
        sorted_ticks = sorted(ticks_data.keys())
        values = [ticks_data[t] for t in sorted_ticks]
        
        # 선 그래프 추가
        fig.add_trace(go.Scatter(
            x=sorted_ticks, 
            y=values, 
            mode='lines+markers', 
            name=char_name,
            marker=dict(size=4),
            hovertemplate='<b>Tick: %{x}</b><br>Urgency: %{y:.4f}<extra></extra>'
        ))

    # 레이아웃 설정
    fig.update_layout(
        title='Total Disutility (Need Urgency Multipliers) Over Time',
        xaxis_title='Tick (Frame)',
        yaxis_title='Total Urgency Score (Lower is Better)',
        legend_title='Characters',
        hovermode='x unified', # 마우스를 올리면 같은 X축 선상의 모든 데이터 표시
        template='plotly_white' # 깔끔한 흰색 테마
    )

    # 그래프 출력
    fig.show()

if __name__ == "__main__":
    plot_total_disutility_plotly('../Assets/Log/Log.json')