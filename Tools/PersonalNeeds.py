import json
import plotly.graph_objects as go
import plotly.io as pio
from common import *
from collections import defaultdict

import os
import json

company_name = "DefaultCompany"
product_name = "School Sim"
log_filename = "log.json"

if os.name == 'nt':  
    base_path = os.path.join(os.environ['USERPROFILE'], 'AppData', 'LocalLow')
else: 
    print("Sorry. Windows Support Only.")
    exit(0)

file_path = os.path.join(base_path, company_name, product_name, "Log", log_filename)

print(f"불러올 경로: {file_path}")


pio.renderers.default = "browser"

def plot_individual_needs_with_dropdown(json_file_path):
    try:
        with open(json_file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except FileNotFoundError:
        print(f"파일을 찾을 수 없습니다: {json_file_path}")
        return

    stat_keys = [
        'hungry', 'fatigue', 'toilet', 'hygiene', 
        'fun', 'loneliness', 'rLoneliness', 'motivation'
    ]

    char_data = defaultdict(lambda: defaultdict(lambda: {'ticks': [], 'values': []}))

    for entry in data.get('chDatas', []):
        tick = entry['tick']
        char_name = entry['charName']
        stats = entry.get('stats', {})

        for key in stat_keys:
            if key in stats:
                val = stats[key]
                val = clamp(val, 0, 100)
                char_data[char_name][key]['ticks'].append(tick)
                char_data[char_name][key]['values'].append(val)

    characters = list(char_data.keys())
    if not characters:
        print("그래프를 그릴 캐릭터 데이터가 없습니다.")
        return

    fig = go.Figure()

    traces_per_char = len(stat_keys)
    
    for i, char_name in enumerate(characters):
        for key in stat_keys:
            is_visible = (i == 0)
            
            fig.add_trace(go.Scatter(
                x=char_data[char_name][key]['ticks'],
                y=char_data[char_name][key]['values'],
                mode='lines+markers',
                name=key, 
                visible=is_visible,
                marker=dict(size=4)
            ))

    buttons = []
    for i, char_name in enumerate(characters):
        visibility = [False] * (len(characters) * traces_per_char)
        
        start_idx = i * traces_per_char
        for j in range(traces_per_char):
            visibility[start_idx + j] = True
            
        button = dict(
            label=char_name,  
            method="update",  
            args=[
                {"visible": visibility},
                {"title": f"[{char_name}] Individual Need Changes Over Time"} 
            ]
        )
        buttons.append(button)

    fig.update_layout(
        updatemenus=[dict(
            active=0,
            buttons=buttons,
            x=1.0,       
            y=1.15,    
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
        template="plotly_white", 
        hovermode="x unified"
    )

    fig.show()

if __name__ == "__main__":
    plot_individual_needs_with_dropdown(file_path)