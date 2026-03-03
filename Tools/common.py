def clamp(x, _min, _max) :
    return max(min(x, _max), _min)

def calc_e_need_multiplier(val):
    val = clamp(val, 0, 100)
    return 0.3 ** ((val - 50.0) / 3.0)

def calc_r_need_multiplier(val):
    val = clamp(val, 0, 100)
    return 1000.0 + ((max(0, 100-val) ** 3))

def calc_g_need_multiplier(val):
    val = clamp(val, 0, 100)
    return 2000 * max(0, 100.0 - val) + 100