
import json
import statistics
from collections import defaultdict
from common import *
import os
import json

company_name = "DefaultCompany"
product_name = "School Sim"
log_filename = "Log.json"

if os.name == 'nt': 
    base_path = os.path.join(os.environ['USERPROFILE'], 'AppData', 'LocalLow')
else:  
    print("Sorry. Windows Support Only.")
    exit(0)

file_path = os.path.join(base_path, company_name, product_name, "Log", log_filename)

print(f"불러올 경로: {file_path}")
def decode_mbti(mbti_val):
    is_I = (mbti_val & 0b1000) != 0
    is_N = (mbti_val & 0b0100) != 0
    is_F = (mbti_val & 0b0010) != 0
    is_P = (mbti_val & 0b0001) != 0
    
    return {
        'E_I': 'I' if is_I else 'E',
        'S_N': 'N' if is_N else 'S',
        'T_F': 'F' if is_F else 'T',
        'J_P': 'P' if is_P else 'J',
        'Full': f"{'I' if is_I else 'E'}{'N' if is_N else 'S'}{'F' if is_F else 'T'}{'P' if is_P else 'J'}"
    }

def analyze_all_mbti(json_file_path):
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    analysis = defaultdict(lambda: {
        'r_needs': [], 'e_needs': [], 'g_needs': [], 
        'total_disutilities': [], 'relations': []
    })
    char_mbti_map = {}
    
    for entry in data.get('chDatas', []):
        char_name = entry['charName']
        stats = entry.get('stats', {})
        char_mbti_map[char_name] = entry.get('mbti', 0)
        
        r_score = sum(calc_r_need_multiplier(stats.get(k, 100)) for k in ['fun', 'loneliness', 'rLoneliness'])
        e_score = sum(calc_e_need_multiplier(stats.get(k, 100)) for k in ['hungry', 'fatigue', 'toilet', 'hygiene'])
        g_score = sum(calc_g_need_multiplier(stats.get(k, 100)) for k in ['motivation'])
        
        analysis[char_name]['r_needs'].append(r_score)
        analysis[char_name]['e_needs'].append(e_score)
        analysis[char_name]['g_needs'].append(g_score)
        analysis[char_name]['total_disutilities'].append(e_score + r_score + g_score)
        
        relations = entry.get('relations', [])
        if relations:
            avg_rel = sum(r.get('val', 0) for r in relations) / len(relations)
            analysis[char_name]['relations'].append(avg_rel)

    stats_E, stats_I = [], []
    stats_S, stats_N = [], []
    anti_S, anti_N = [], []
    stats_T, stats_F = [], []
    stats_J, stats_P = [], []

    print("=== 캐릭터별 지표 분석 ===")
    for char_name, metrics in analysis.items():
        traits = decode_mbti(char_mbti_map[char_name])
        
        avg_r = sum(metrics['r_needs']) / len(metrics['r_needs'])
        avg_e = sum(metrics['e_needs']) / len(metrics['e_needs'])
        avg_g = sum(metrics['g_needs']) / len(metrics['g_needs'])
        variance = statistics.variance(metrics['total_disutilities']) if len(metrics['total_disutilities']) > 1 else 0
        avg_relation = sum(metrics['relations']) / len(metrics['relations']) if metrics['relations'] else 0
        
        print(f"[{char_name} ({traits['Full']})]")
        print(f"  - (E/I) 관계 불만족도: {avg_r:.2f}")
        print(f"  - (S/N) 생존/동기 비율(E/G): {avg_e:.2f} / {avg_g:.2f}")
        
        if traits['E_I'] == 'E': stats_E.append(avg_r)
        else: stats_I.append(avg_r)
            
        if traits['S_N'] == 'S':
            stats_S.append(avg_e)
            anti_N.append(avg_g)
        else: 
            stats_N.append(avg_g)
            anti_S.append(avg_e)

    print("=== MBTI 4대 지표 그룹별 통계 검증 ===")
    
    print("\n1. [E vs I] 외향 vs 내향 (관계 욕구 불만족도)")
    if stats_E and stats_I:
        print(f"   E({sum(stats_E)/len(stats_E):.2f}) vs I({sum(stats_I)/len(stats_I):.2f})  => E가 더 낮으면(사교적이면) 성공")
        if sum(stats_E)/len(stats_E) < sum(stats_I)/len(stats_I):
            print("      sucess")


    print("\n2. [S vs N] 감각 vs 직관 (우선순위 욕구 집중도)")
    if stats_S and stats_N:
        print(f"   S의 생존 불만족도: {sum(stats_S)/len(stats_S):.2f} (낮을수록 현실적)")
        print(f"      N의 생존 불만족도: {sum(anti_S)/len(anti_S):.2f}")
        print(f"   N의 동기 불만족도: {sum(stats_N)/len(stats_N):.2f} (낮을수록 이상적)")
        print(f"      S의 동기 불만족도: {sum(anti_N)/len(anti_N):.2f}")
        print("   => S는 E-Need를, N은 G-Need를 더 잘 방어하면 성공")
        if sum(stats_S)/len(stats_S) < sum(anti_S)/len(anti_S) and sum(stats_N)/len(stats_N) < sum(anti_N)/len(anti_N):
            print("      sucess")

if __name__ == "__main__":
    analyze_all_mbti(file_path)
    pass
