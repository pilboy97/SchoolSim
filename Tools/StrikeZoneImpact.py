import json
import numpy as np
import plotly.graph_objects as go
import plotly.io as pio
from plotly.subplots import make_subplots
from collections import defaultdict
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

def analyze_strike_zone_impact(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    if not data.get('chDatas'):
        print("No Data to analyze.")
        return

    last_tick = max(entry['tick'] for entry in data['chDatas'])
    final_data = [e for e in data['chDatas'] if e['tick'] == last_tick]

    char_stats = {}
    rel_matrix = defaultdict(dict)

    for entry in final_data:
        name = entry['charName']
        char_id = entry['id']
        char_stats[char_id] = {
            'name': name,
            'attraction': entry.get('attraction', 0.0),
            'e': entry.get('e', 0.0),                  
        }
        
        for rel in entry.get('relations', []):
            target_id = rel['rel']['ID']
            if rel['rel']['relType'] == 0: 
                rel_matrix[char_id][target_id] = rel['val']

    names, attractions, e_values = [], [], []
    incoming_rels = [] 
    outgoing_rels = [] 
    
    for c_id, stats in char_stats.items():
        names.append(stats['name'])
        attractions.append(stats['attraction'])
        e_values.append(stats['e'])
        
        incoming = [rel_matrix[o_id].get(c_id, 0) for o_id in char_stats if o_id != c_id]
        incoming_rels.append(np.mean(incoming) if incoming else 0)
        
        outgoing = [rel_matrix[c_id].get(o_id, 0) for o_id in char_stats if o_id != c_id]
        outgoing_rels.append(np.mean(outgoing) if outgoing else 0)

    corr_attr_incoming = np.corrcoef(attractions, incoming_rels)[0, 1] if len(attractions) > 1 else 0
    corr_e_outgoing = np.corrcoef(e_values, outgoing_rels)[0, 1] if len(e_values) > 1 else 0

    print("=== ⚾ Proof of Concept : Impact of Strike Zone ===")
    print(f"1. Attraction (b) vs Population Correlation: {corr_attr_incoming:.2f}")
    print(f"   -> positive value is expected. The more attractive, the more popular.")
    
    print(f"2. pickiness (e) vs atttactive of others 상관계수: {corr_e_outgoing:.2f}")
    print(f"   -> negative is expected. The more closer e value is to 1, the less probablity to like others.")

    asymmetry_scores = []
    char_ids = list(char_stats.keys())
    for i in range(len(char_ids)):
        for j in range(i + 1, len(char_ids)):
            id1, id2 = char_ids[i], char_ids[j]
            a_to_b = rel_matrix[id1].get(id2, 0)
            b_to_a = rel_matrix[id2].get(id1, 0)
            asymmetry_scores.append(abs(a_to_b - b_to_a))
    
    print(f"3. asymmetry: {np.mean(asymmetry_scores):.2f}")
    print(f"   -> expected bigger than zero. (we have difference of the level of attraction and the ideal type of relation)")

    fig = make_subplots(
        rows=1, cols=2, 
        subplot_titles=(
            f"Attraction vs Popularity (Corr: {corr_attr_incoming:.2f})", 
            f"Eccentricity (e) vs Pickiness (Corr: {corr_e_outgoing:.2f})"
        )
    )

    fig.add_trace(go.Scatter(
        x=attractions, y=incoming_rels,
        mode='markers+text',
        name='Characters',
        text=names,
        textposition='top right',
        marker=dict(size=12, color='royalblue', opacity=0.7),
        hovertemplate="<b>%{text}</b><br>Attraction: %{x:.2f}<br>Popularity: %{y:.2f}<extra></extra>"
    ), row=1, col=1)

    if len(attractions) > 1:
        z1 = np.polyfit(attractions, incoming_rels, 1)
        p1 = np.poly1d(z1)
        x_range1 = np.linspace(min(attractions), max(attractions), 100)
        fig.add_trace(go.Scatter(
            x=x_range1, y=p1(x_range1),
            mode='lines',
            line=dict(color='red', dash='dash', width=2),
            name='Trendline (Attraction)',
            hoverinfo='skip'
        ), row=1, col=1)

    fig.add_trace(go.Scatter(
        x=e_values, y=outgoing_rels,
        mode='markers+text',
        name='Characters',
        text=names,
        textposition='top right',
        marker=dict(size=12, color='crimson', opacity=0.7),
        hovertemplate="<b>%{text}</b><br>Eccentricity (e): %{x:.2f}<br>Pickiness (Outgoing): %{y:.2f}<extra></extra>"
    ), row=1, col=2)

    if len(e_values) > 1:
        z2 = np.polyfit(e_values, outgoing_rels, 1)
        p2 = np.poly1d(z2)
        x_range2 = np.linspace(min(e_values), max(e_values), 100)
        fig.add_trace(go.Scatter(
            x=x_range2, y=p2(x_range2),
            mode='lines',
            line=dict(color='blue', dash='dash', width=2),
            name='Trendline (Eccentricity)',
            hoverinfo='skip'
        ), row=1, col=2)

    fig.update_layout(
        title_text="⚾ Strike Zone System Impact Analysis",
        template="plotly_white",
        showlegend=False,
        height=600,
        hovermode="closest"
    )

    fig.update_xaxes(title_text="Attraction (Ellipsis b-axis)", row=1, col=1)
    fig.update_yaxes(title_text="Avg Incoming Relation (Popularity)", row=1, col=1)
    
    fig.update_xaxes(title_text="Eccentricity 'e' (Closer to 1 = Picky)", row=1, col=2)
    fig.update_yaxes(title_text="Avg Outgoing Relation (How much they like others)", row=1, col=2)

    fig.show()

if __name__ == "__main__":

    if len(sys.argv) > 1:
        file_path = sys.argv[1]

    analyze_strike_zone_impact(file_path)
    pass