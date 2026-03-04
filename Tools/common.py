def clamp(x, _min, _max) :
    return max(min(x, _max), _min)

def calc_e_need_multiplier(val):
    val = clamp(val, 0, 100)
    diff = 100 - val
    return 0.3 ** (-(diff - 50) / 3.0)

def calc_r_need_multiplier(val):
    val = clamp(val, 0, 100)
    diff = 100 - val
    return diff ** 3

def calc_g_need_multiplier(val):
    val = clamp(val, 0, 100)
    diff = 100 - val
    return 2000 * diff + 1000