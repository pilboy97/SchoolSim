import json
import plotly.graph_objects as go
import plotly.io as pio
from common import *
from collections import defaultdict

pio.renderers.default = "browser"

def plot_individual_needs_with_dropdown(json_file_path):
    # 1. JSON 로그 데이터 로드
    try:
        with open(json_file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except FileNotFoundError:
        print(f"파일을 찾을 수 없습니다: {json_file_path}")
        return

    # 추적할 8가지 욕구 스탯 리스트
    stat_keys = [
        'hungry', 'fatigue', 'toilet', 'hygiene', 
        'fun', 'loneliness', 'rLoneliness', 'motivation'
    ]

    # 데이터 구조: char_data[캐릭터이름][욕구종류] = {'ticks': [], 'values': []}
    char_data = defaultdict(lambda: defaultdict(lambda: {'ticks': [], 'values': []}))

    # 2. 데이터 파싱
    for entry in data.get('chDatas', []):
        tick = entry['tick']
        char_name = entry['charName']
        stats = entry.get('stats', {})

        for key in stat_keys:
            if key in stats:
                val = stats[key]
                val = clamp(val, 0, 100)
                char_data[char_name][key]['ticks'].append(tick)
                char_data[char_name][key]['values'].append(stats[key])

    characters = list(char_data.keys())
    if not characters:
        print("그래프를 그릴 캐릭터 데이터가 없습니다.")
        return

    # 3. Plotly Figure 생성
    fig = go.Figure()

    traces_per_char = len(stat_keys)
    
    # 4. 모든 캐릭터의 모든 스탯 Trace(선)를 Figure에 추가
    for i, char_name in enumerate(characters):
        for key in stat_keys:
            # 기본적으로 첫 번째 캐릭터의 그래프만 보이도록 설정 (나머지는 숨김)
            is_visible = (i == 0)
            
            fig.add_trace(go.Scatter(
                x=char_data[char_name][key]['ticks'],
                y=char_data[char_name][key]['values'],
                mode='lines+markers',
                name=key, # 범례 이름
                visible=is_visible,
                marker=dict(size=4)
            ))

    # 5. 드롭다운(Dropdown) 버튼 생성 로직
    buttons = []
    for i, char_name in enumerate(characters):
        # 특정 캐릭터가 선택되었을 때 어떤 선들을 보여주고 숨길지 결정하는 boolean 배열
        visibility = [False] * (len(characters) * traces_per_char)
        
        start_idx = i * traces_per_char
        for j in range(traces_per_char):
            visibility[start_idx + j] = True
            
        # 버튼 하나 생성
        button = dict(
            label=char_name,  # 드롭다운에 표시될 이름
            method="update",  # 업데이트 방식
            args=[
                {"visible": visibility}, # 선 가시성 업데이트
                {"title": f"[{char_name}] Individual Need Changes Over Time"} # 타이틀 업데이트
            ]
        )
        buttons.append(button)

    # 6. 레이아웃(UI) 설정
    fig.update_layout(
        updatemenus=[dict(
            active=0,
            buttons=buttons,
            x=1.0,         # 드롭다운 X 위치 (1.0 = 우측 끝)
            y=1.15,        # 드롭다운 Y 위치 (1.0 = 상단 끝)
            xanchor="right",
            yanchor="top",
            direction="down",
            showactive=True,
        )],
        title=f"[{characters[0]}] Individual Need Changes Over Time",
        xaxis_title="Tick (Frame)",
        yaxis_title="Need Stat Value (0 ~ 100)",
        legend_title="Need Types",
        font=dict(size=12),
        template="plotly_white", # 깔끔한 흰색 배경 테마
        hovermode="x unified" # 마우스를 올리면 해당 틱의 모든 욕구 수치를 한 번에 보여줌
    )

    # 7. HTML로 렌더링 후 브라우저에서 열기
    fig.show()

if __name__ == "__main__":
    # 추출한 JSON 파일 경로를 맞게 수정해주세요.
    plot_individual_needs_with_dropdown('../Assets/Log/Log.json')