import json
import plotly.graph_objects as go
from collections import defaultdict
from common import *
import os
import sys
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

def plot_total_disutility_plotly(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    e_needs = ['hungry', 'fatigue', 'toilet', 'hygiene']
    r_needs = ['fun', 'loneliness', 'rLoneliness']
    g_needs = ['motivation']
    
    char_data = defaultdict(lambda: defaultdict(float))
    
    max_e_need_multiplier = calc_e_need_multiplier(0)
    max_r_need_multiplier = calc_r_need_multiplier(0)
    max_g_need_multiplier = calc_g_need_multiplier(0)

    for entry in data.get('chDatas', []):
        tick = entry['tick']
        char_name = entry['charName']
        stats = entry.get('stats', {})
        
        total_disutility = 0.0

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

    fig = go.Figure()

    for char_name, ticks_data in char_data.items():
        sorted_ticks = sorted(ticks_data.keys())
        values = [ticks_data[t] for t in sorted_ticks]
        
        fig.add_trace(go.Scatter(
            x=sorted_ticks, 
            y=values, 
            mode='lines+markers', 
            name=char_name,
            marker=dict(size=4),
            hovertemplate='<b>Tick: %{x}</b><br>Urgency: %{y:.4f}<extra></extra>'
        ))

    fig.update_layout(
        title='Total Disutility (Need Urgency Multipliers) Over Time',
        xaxis_title='Tick (Frame)',
        yaxis_title='Total Urgency Score (Lower is Better)',
        legend_title='Characters',
        hovermode='x unified',
    )

    fig.show()

if __name__ == "__main__":

    if len(sys.argv) > 1:
        file_path = sys.argv[1]

    plot_total_disutility_plotly(file_path)