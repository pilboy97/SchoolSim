import json
import plotly.graph_objects as go
import plotly.io as pio
from collections import defaultdict
from common import *

# 웹 브라우저 렌더러 설정
pio.renderers.default = "browser"

def plot_needs_variance_plotly(json_file_path):
    # 1. JSON 로그 데이터 로드
    try:
        with open(json_file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except FileNotFoundError:
        print(f"파일을 찾을 수 없습니다: {json_file_path}")
        return

    # 욕구(Need)로 간주할 스탯 키 목록
    need_keys = [
        'hungry', 'fatigue', 'toilet', 'hygiene', 
        'loneliness', 'rLoneliness', 'fun', 'motivation'
    ]
    
    # 캐릭터별 데이터를 저장할 딕셔너리: charName -> {'ticks': [], 'values': []}
    char_data = defaultdict(lambda: {'ticks': [], 'values': []})
    
    # 2. 데이터 파싱 및 분산 평균 계산
    for entry in data.get('chDatas', []):
        tick = entry['tick']
        char_name = entry['charName']
        stats = entry.get('stats', {})
        
        sum_sq_diff = 0
        count = 0
        
        # 정의된 need 스탯들에 대해 (100 - need)^2 계산
        for key in need_keys:
            if key in stats:
                val = stats[key]
                val = clamp(val, 0, 100)
                sum_sq_diff += (100 - val) ** 2
                count += 1
                
        # 평균 계산 후 저장
        if count > 0:
            avg_sq_diff = sum_sq_diff / count
            char_data[char_name]['ticks'].append(tick)
            char_data[char_name]['values'].append(avg_sq_diff)

    if not char_data:
        print("그래프를 그릴 데이터가 없습니다.")
        return

    # 3. Plotly Figure 생성
    fig = go.Figure()

    # 4. 각 캐릭터별로 선(Trace) 추가
    for char_name, metrics in char_data.items():
        # 시간(Tick) 순으로 정렬 보장
        sorted_pairs = sorted(zip(metrics['ticks'], metrics['values']))
        if not sorted_pairs:
            continue
            
        ticks, values = zip(*sorted_pairs)

        fig.add_trace(go.Scatter(
            x=ticks,
            y=values,
            mode='lines+markers',
            name=char_name,
            marker=dict(size=4),
            hovertemplate="<b>%{text}</b>: %{y:.2f}<extra></extra>",
            text=[char_name] * len(ticks) # 툴팁에 캐릭터 이름 표시
        ))

    # 5. 레이아웃(UI) 설정
    fig.update_layout(
        title="Average of (100 - Need)² Over Time (Homeostasis Stability)",
        xaxis_title="Tick (Frame)",
        yaxis_title="Avg( (100 - Need)² ) (Lower is More Stable)",
        legend_title="Characters",
        font=dict(size=12),
        template="plotly_white", # 깔끔한 흰색 배경 테마
        hovermode="x unified"    # 핵심 기능: 마우스를 올리면 해당 Tick의 모든 캐릭터 수치를 한 번에 보여줌
    )

    # 6. HTML로 렌더링 후 브라우저에서 열기
    fig.show()

if __name__ == "__main__":
    # 추출한 JSON 파일 경로를 맞게 수정해주세요.
    plot_needs_variance_plotly('../Assets/Log/Log.json')