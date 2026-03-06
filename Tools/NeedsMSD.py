import json
import plotly.graph_objects as go
import plotly.io as pio
from collections import defaultdict
from common import *
import os
import json
import sys

company_name = "DefaultCompany"
product_name = "School Sim"
log_filename = "log.json"

if os.name == 'nt':  
    base_path = os.path.join(os.environ['USERPROFILE'], 'AppData', 'LocalLow')
else: 
    print("Sorry. Windows Support Only.")
    exit(0)

file_path = os.path.join(base_path, company_name, product_name, "Log", log_filename)

print(f"Load File: {file_path}")

pio.renderers.default = "browser"

def plot_needs_variance_plotly(json_file_path):
    try:
        with open(json_file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except FileNotFoundError:
        print(f"Cannot Find file: {json_file_path}")
        return

    need_keys = [
        'hungry', 'fatigue', 'toilet', 'hygiene', 
        'loneliness', 'rLoneliness', 'fun', 'motivation'
    ]
    
    char_data = defaultdict(lambda: {'ticks': [], 'values': []})
    
    for entry in data.get('chDatas', []):
        tick = entry['tick']
        char_name = entry['charName']
        stats = entry.get('stats', {})
        
        sum_sq_diff = 0
        count = 0
        
        for key in need_keys:
            if key in stats:
                val = stats[key]
                val = clamp(val, 0, 100)
                sum_sq_diff += (100 - val) ** 2
                count += 1
                
        if count > 0:
            avg_sq_diff = sum_sq_diff / count
            char_data[char_name]['ticks'].append(tick)
            char_data[char_name]['values'].append(avg_sq_diff)

    if not char_data:
        print("No data to plot.")
        return

    fig = go.Figure()

    for char_name, metrics in char_data.items():
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
            text=[char_name] * len(ticks)
        ))

    fig.update_layout(
        title="Average of (100 - Need)² Over Time (Homeostasis Stability)",
        xaxis_title="Tick (Frame)",
        yaxis_title="Avg( (100 - Need)² ) (Lower is More Stable)",
        legend_title="Characters",
        font=dict(size=12),
        template="plotly_white", 
        hovermode="x unified"    
    )

    fig.show()

if __name__ == "__main__":
    if len(sys.argv) > 1:
        file_path = sys.argv[1]
        
    plot_needs_variance_plotly(file_path)