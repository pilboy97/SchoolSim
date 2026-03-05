import json
import numpy as np
import plotly.graph_objects as go
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

print(f"불러올 경로: {file_path}")

def plot_relationship_network_safe(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    ch_datas = data.get('chDatas', [])
    if not ch_datas: return

    ticks = sorted(list(set(e['tick'] for e in ch_datas)))
    char_ids = sorted(list(set(e['id'] for e in ch_datas)))
    id_to_name = {e['id']: e['charName'] for e in ch_datas}
    
    all_pairs = []
    for i in range(len(char_ids)):
        for j in range(i + 1, len(char_ids)):
            all_pairs.append((char_ids[i], char_ids[j]))

    pos = {cid: np.array([np.cos(2*np.pi*i/len(char_ids)), 
                          np.sin(2*np.pi*i/len(char_ids))]) * 0.5 + np.random.normal(0, 0.01, 2)
           for i, cid in enumerate(char_ids)}

    frames = []
    for tick in ticks:
        tick_entries = {e['id']: e for e in ch_datas if e['tick'] == tick}
        
        for _ in range(5):
            for cid in char_ids:
                center_dist = np.linalg.norm(pos[cid])
                if center_dist > 0.01:
                    pos[cid] -= (pos[cid] / center_dist) * (center_dist * 0.1)

            for i in range(len(char_ids)):
                for j in range(i + 1, len(char_ids)):
                    id1, id2 = char_ids[i], char_ids[j]
                    diff = pos[id1] - pos[id2]
                    dist = np.linalg.norm(diff) + 0.1 
                    
                    force_mag = min(0.1, 0.05 / (dist**2)) 
                    force = (diff / dist) * force_mag
                    pos[id1] += force
                    pos[id2] -= force

            for cid in char_ids:
                if cid in tick_entries:
                    for rel in tick_entries[cid].get('relations', []):
                        target_id = rel['rel']['ID']
                        if target_id not in pos: continue
                        
                        val = rel['val']
                        diff = pos[target_id] - pos[cid]
                        dist = np.linalg.norm(diff) + 0.01
                        
                        is_romance = (rel['rel']['relType'] == 1)
                        strength = np.clip(val / 5000.0, -0.05, 0.05) 
                        if is_romance: strength *= 2.0
                        
                        pos[cid] += diff * strength

            for cid in char_ids:
                pos[cid] = np.clip(pos[cid], -10, 10)

        lines = {'romance': {'x': [], 'y': []}, 'friend': {'x': [], 'y': []}, 'negative': {'x': [], 'y': []}}
        for p1, p2 in all_pairs:
            rel = None
            if p1 in tick_entries:
                rel = next((r for r in tick_entries[p1].get('relations', []) if r['rel']['ID'] == p2), None)
            if rel and abs(rel['val']) > 0.1:
                cat = 'romance' if rel['rel']['relType'] == 1 else ('friend' if rel['val'] > 0 else 'negative')
                lines[cat]['x'].extend([pos[p1][0], pos[p2][0], None])
                lines[cat]['y'].extend([pos[p1][1], pos[p2][1], None])

        frame_traces = [
            go.Scatter(x=lines['romance']['x'], y=lines['romance']['y'], mode='lines', line=dict(width=4, color='#FF1744')),
            go.Scatter(x=lines['friend']['x'], y=lines['friend']['y'], mode='lines', line=dict(width=2, color='#00C853')),
            go.Scatter(x=lines['negative']['x'], y=lines['negative']['y'], mode='lines', line=dict(width=1, color='#9E9E9E')),
            go.Scatter(x=[pos[cid][0] for cid in char_ids], y=[pos[cid][1] for cid in char_ids],
                       mode='markers+text', text=[id_to_name[cid] for cid in char_ids],
                       textposition="top center", marker=dict(size=18, color='white', line=dict(width=2, color='#333')))
        ]
        frames.append(go.Frame(data=frame_traces, name=str(tick)))

    fig = go.Figure(
        data=frames[0].data if frames else [],
        layout=go.Layout(
            template="plotly_white",
            xaxis=dict(range=[-5, 5], showgrid=False, zeroline=False, showticklabels=False),
            yaxis=dict(range=[-5, 5], showgrid=False, zeroline=False, showticklabels=False),
            updatemenus=[{"type": "buttons", "buttons": [{"label": "▶ Play", "method": "animate", "args": [None, {"frame": {"duration": 40, "redraw": False}}]}]}]
        ),
        frames=frames
    )
    fig.show()

if __name__ == "__main__":

    if len(sys.argv) > 1:
        file_path = sys.argv[1]

    plot_relationship_network_safe(file_path)