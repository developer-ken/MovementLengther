﻿namespace FashionStar.Servo.Uart
{
    public static class TemperatureConverter
    {
        private static double[] _ntcr = new double[] {
        2282376,
        2148696,
        2023826,
        1907126,
        1798005,
        1695919,
        1600366,
        1510884,
        1427046,
        1348459,
        1274759,
        1205608,
        1140696,
        1079735,
        1022459,
        968620,
        917990,
        870357,
        825523,
        783306,
        743538,
        706058,
        670723,
        637394,
        605946,
        576261,
        548228,
        521745,
        496717,
        473056,
        450676,
        429503,
        409462,
        390487,
        372514,
        355484,
        339342,
        324037,
        309520,
        295745,
        282671,
        270257,
        258466,
        247264,
        236617,
        226495,
        216869,
        207711,
        198996,
        190700,
        182801,
        175276,
        168108,
        161275,
        154762,
        148550,
        142625,
        136972,
        131576,
        126425,
        121505,
        116806,
        112316,
        108025,
        103923,
        100000,
        96248,
        92658,
        89223,
        85934,
        82786,
        79770,
        76882,
        74114,
        71461,
        68919,
        66480,
        64142,
        61899,
        59746,
        57680,
        55697,
        53793,
        51964,
        50208,
        48520,
        46898,
        45339,
        43840,
        42398,
        41012,
        39678,
        38395,
        37160,
        35971,
        34826,
        33724,
        32662,
        31639,
        30654,
        29704,
        28788,
        27905,
        27054,
        26233,
        25442,
        24678,
        23940,
        23229,
        22542,
        21879,
        21239,
        20620,
        20023,
        19446,
        18888,
        18349,
        17828,
        17324,
        16837,
        16366,
        15910,
        15469,
        15043,
        14630,
        14231,
        13844,
        13470,
        13107,
        12756,
        12416,
        12087,
        11768,
        11459,
        11159,
        10868,
        10587,
        10314,
        10049,
        9792,
        9543,
        9302,
        9067,
        8840,
        8619,
        8405,
        8197,
        7995,
        7799,
        7608,
        7423,
        7244,
        7069,
        6900,
        6735,
        6575,
        6419,
        6268,
        6121,
        5978,
        5839,
        5703,
        5572,
        5444,
        5319,
        5198};

        public static int DegreeToAdc(int degree)
        {
            if (degree == 0) return 0;

            double resistance = _ntcr[degree + 40];
            double tempAdc = resistance * 4096 / (100000.0 + resistance) + 0.5;
            int returnValue = (int)tempAdc;

            return returnValue;
        }

        public static int AdcToDegree(int adc)
        {
            if (adc == 0) return 0;

            double ratio = 100000.0 * adc / (4096 - adc);
            int resistance = (int)ratio;
            int i;
            for (i = 0; i < 165; i++)
            {
                if ((resistance <= _ntcr[i]) && (resistance >= _ntcr[i + 1]))
                    break;
            }

            int returnValue = i - 40;

            try
            {
                if (_ntcr[i] - resistance > resistance - _ntcr[i + 1])
                {
                    returnValue++;
                }
            }
            catch
            {
            }

            return returnValue;
        }
    }
}
