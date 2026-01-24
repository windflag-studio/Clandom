using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Clandom.Models.BalancedRandom
{
    /// <summary>
    /// 平衡随机抽取类，提供智能动态权重算法和平均值差值保护机制
    /// </summary>
    public class BalancedRand
    {
        // 内部数据结构
        private Dictionary<int, int> _drawCounts;  // 学号 -> 抽取次数
        private Dictionary<int, int> _lastDrawRound;  // 学号 -> 最后被抽中的轮次
        private List<int> _allNumbers;  // 所有学号
        private List<int>? _candidatePool;  // 当前候选池
        private Random _random;
        
        // 配置参数
        private int _currentRound;  // 当前抽取轮次
        private int _minPoolSize;  // 最小候选池大小
        private int _maxGapThreshold;  // 最大差距阈值
        private double _coldStartBoost;  // 冷启动提升系数
        private double _decayFactor;  // 权重衰减因子
        
        // 统计信息
        private int _totalDraws;
        private Dictionary<int, double> _currentProbabilities;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="numberRangeStart">学号起始值</param>
        /// <param name="numberRangeEnd">学号结束值</param>
        /// <param name="minPoolSize">最小候选池大小（默认3）</param>
        /// <param name="maxGapThreshold">最大抽取次数差距阈值（默认5）</param>
        /// <param name="coldStartBoost">冷启动提升系数（默认2.0）</param>
        /// <param name="decayFactor">权重衰减因子（默认0.7）</param>
        public BalancedRand(int numberRangeStart, int numberRangeEnd, 
                           int minPoolSize = 3, int maxGapThreshold = 5,
                           double coldStartBoost = 2.0, double decayFactor = 0.7)
        {
            if (numberRangeStart > numberRangeEnd)
                throw new ArgumentException("起始值不能大于结束值");
            
            if (minPoolSize < 1)
                throw new ArgumentException("最小候选池大小必须大于0");
                
            _allNumbers = Enumerable.Range(numberRangeStart, numberRangeEnd - numberRangeStart + 1).ToList();
            _drawCounts = _allNumbers.ToDictionary(n => n, n => 0);
            _lastDrawRound = _allNumbers.ToDictionary(n => n, n => -1); // -1表示从未被抽中
            _random = new Random(Guid.NewGuid().GetHashCode());
            _currentRound = 0;
            _minPoolSize = minPoolSize;
            _maxGapThreshold = maxGapThreshold;
            _coldStartBoost = coldStartBoost;
            _decayFactor = decayFactor;
            _totalDraws = 0;
            _currentProbabilities = new Dictionary<int, double>();
            
            // 初始化候选池
            UpdateCandidatePool();
        }

        /// <summary>
        /// 构造函数（通过列表指定学号）
        /// </summary>
        /// <param name="numbers">学号列表</param>
        /// <param name="minPoolSize">最小候选池大小</param>
        /// <param name="maxGapThreshold">最大抽取次数差距阈值</param>
        /// <param name="coldStartBoost">冷启动提升系数</param>
        /// <param name="decayFactor">权重衰减因子</param>
        public BalancedRand(IEnumerable<int> numbers,
                           int minPoolSize = 3, int maxGapThreshold = 5,
                           double coldStartBoost = 2.0, double decayFactor = 0.7)
        {
            var enumerable = numbers as int[] ?? numbers.ToArray();
            if (numbers == null || !enumerable.Any())
                throw new ArgumentException("学号列表不能为空");
                
            _allNumbers = enumerable.Distinct().ToList();
            _drawCounts = _allNumbers.ToDictionary(n => n, n => 0);
            _lastDrawRound = _allNumbers.ToDictionary(n => n, n => -1);
            _random = new Random(Guid.NewGuid().GetHashCode());
            _currentRound = 0;
            _minPoolSize = minPoolSize;
            _maxGapThreshold = maxGapThreshold;
            _coldStartBoost = coldStartBoost;
            _decayFactor = decayFactor;
            _totalDraws = 0;
            _currentProbabilities = new Dictionary<int, double>();
            
            UpdateCandidatePool();
        }

        /// <summary>
        /// 抽取一个学号
        /// </summary>
        /// <returns>抽取到的学号</returns>
        public int Draw()
        {
            if (_candidatePool != null && _candidatePool.Count == 0)
            {
                // 如果候选池为空，重置所有抽取次数
                ResetDrawCounts();
            }

            _currentRound++;
            
            // 计算每个候选者的权重
            var weights = CalculateWeights();
            
            // 根据权重进行随机抽取
            int selectedNumber = WeightedRandomSelect(weights);
            
            // 更新抽取记录
            _drawCounts[selectedNumber]++;
            _lastDrawRound[selectedNumber] = _currentRound;
            _totalDraws++;
            
            // 更新候选池和概率
            UpdateCandidatePool();
            UpdateProbabilities();
            
            return selectedNumber;
        }

        /// <summary>
        /// 批量抽取多个学号
        /// </summary>
        /// <param name="count">抽取数量</param>
        /// <returns>抽取到的学号列表</returns>
        public List<int> DrawMultiple(int count)
        {
            if (count <= 0) 
                throw new ArgumentException("抽取数量必须大于0");
            if (_candidatePool != null && count > _candidatePool.Count)
                throw new ArgumentException($"抽取数量不能超过候选池大小({_candidatePool.Count})");
                
            List<int> results = new List<int>();
            
            for (int i = 0; i < count; i++)
            {
                // 每次抽取后候选池会更新，所以需要重新计算
                results.Add(Draw());
            }
            
            return results;
        }

        /// <summary>
        /// 获取当前抽取统计信息
        /// </summary>
        /// <returns>学号->抽取次数字典</returns>
        public Dictionary<int, int> GetStatistics()
        {
            return new Dictionary<int, int>(_drawCounts);
        }

        /// <summary>
        /// 获取当前每个学号的抽取概率
        /// </summary>
        /// <returns>学号->概率字典</returns>
        public Dictionary<int, double> GetProbabilities()
        {
            return new Dictionary<int, double>(_currentProbabilities);
        }

        /// <summary>
        /// 重置所有抽取次数（用于重新开始或手动平衡）
        /// </summary>
        public void ResetDrawCounts()
        {
            foreach (var number in _allNumbers)
            {
                _drawCounts[number] = 0;
                _lastDrawRound[number] = -1;
            }
            _totalDraws = 0;
            _currentRound = 0;
            UpdateCandidatePool();
        }

        /// <summary>
        /// 获取当前候选池
        /// </summary>
        /// <returns>候选池学号列表</returns>
        public List<int> GetCandidatePool()
        {
            Debug.Assert(_candidatePool != null, nameof(_candidatePool) + " != null");
            return new List<int>(_candidatePool);
        }

        /// <summary>
        /// 获取平均抽取次数
        /// </summary>
        /// <returns>平均抽取次数</returns>
        public double GetAverageDrawCount()
        {
            return _allNumbers.Count > 0 ? (double)_totalDraws / _allNumbers.Count : 0;
        }

        /// <summary>
        /// 获取最大抽取次数差距
        /// </summary>
        /// <returns>最大差距</returns>
        public int GetMaxDrawCountGap()
        {
            if (_drawCounts.Count == 0) return 0;
            int max = _drawCounts.Values.Max();
            int min = _drawCounts.Values.Min();
            return max - min;
        }

        /// <summary>
        /// 更新配置参数
        /// </summary>
        public void UpdateParameters(int? minPoolSize = null, int? maxGapThreshold = null,
                                   double? coldStartBoost = null, double? decayFactor = null)
        {
            if (minPoolSize.HasValue && minPoolSize.Value > 0)
                _minPoolSize = minPoolSize.Value;
                
            if (maxGapThreshold.HasValue && maxGapThreshold.Value >= 0)
                _maxGapThreshold = maxGapThreshold.Value;
                
            if (coldStartBoost.HasValue && coldStartBoost.Value >= 1.0)
                _coldStartBoost = coldStartBoost.Value;
                
            if (decayFactor.HasValue && decayFactor.Value > 0 && decayFactor.Value <= 1.0)
                _decayFactor = decayFactor.Value;
                
            UpdateCandidatePool();
        }

        #region 私有方法

        /// <summary>
        /// 更新候选池
        /// </summary>
        private void UpdateCandidatePool()
        {
            // 计算平均抽取次数
            double average = GetAverageDrawCount();
            
            // 第一步：平均值过滤 - 只选择抽取次数≤平均值的成员
            var candidates = _allNumbers
                .Where(n => _drawCounts[n] <= Math.Ceiling(average)) // 向上取整，增加容错
                .ToList();
            
            // 第二步：最大差距保护
            if (GetMaxDrawCountGap() > _maxGapThreshold)
            {
                // 排除极值并重新计算
                int maxCount = _drawCounts.Values.Max();
                int minCount = _drawCounts.Values.Min();
                
                // 排除抽取次数最多和最少的成员
                candidates = candidates
                    .Where(n => _drawCounts[n] != maxCount && _drawCounts[n] != minCount)
                    .ToList();
                
                // 重新计算排除极值后的平均值
                if (candidates.Any())
                {
                    double newAverage = candidates.Average(n => _drawCounts[n]);
                    candidates = candidates
                        .Where(n => _drawCounts[n] <= Math.Ceiling(newAverage))
                        .ToList();
                }
            }
            
            // 第三步：候选池大小保障
            if (candidates.Count < _minPoolSize)
            {
                // 如果候选池太小，添加一些抽取次数较低的成员
                var allSorted = _allNumbers
                    .OrderBy(n => _drawCounts[n])
                    .ThenBy(n => _lastDrawRound[n]) // 长期未抽中的优先
                    .ToList();
                    
                int needed = _minPoolSize - candidates.Count;
                foreach (var number in allSorted)
                {
                    if (!candidates.Contains(number) && needed > 0)
                    {
                        candidates.Add(number);
                        needed--;
                    }
                }
            }
            
            _candidatePool = candidates;
        }

        /// <summary>
        /// 计算权重
        /// </summary>
        private Dictionary<int, double> CalculateWeights()
        {
            var weights = new Dictionary<int, double>();

            if (_candidatePool != null)
                foreach (var number in _candidatePool)
                {
                    double weight = 1.0;

                    // 1. 基础权重：避免重复抽取
                    weight *= Math.Pow(_decayFactor, _drawCounts[number]);

                    // 2. 冷启动保护：长期未被抽中的成员权重提升
                    if (_lastDrawRound[number] < 0) // 从未被抽中
                    {
                        weight *= _coldStartBoost;
                    }
                    else
                    {
                        int roundsSinceLastDraw = _currentRound - _lastDrawRound[number];
                        if (roundsSinceLastDraw > _allNumbers.Count / 2) // 超过一半轮次未抽中
                        {
                            weight *= (1.0 + Math.Log(roundsSinceLastDraw + 1) / 10.0);
                        }
                    }

                    // 3. 抽取次数倒数权重（抽取越多，权重越低）
                    weight *= 1.0 / (_drawCounts[number] + 1);

                    weights[number] = Math.Max(weight, 0.01); // 保证最小权重
                }

            return weights;
        }

        /// <summary>
        /// 根据权重进行随机选择
        /// </summary>
        private int WeightedRandomSelect(Dictionary<int, double> weights)
        {
            if (!weights.Any())
                throw new InvalidOperationException("权重字典为空");
                
            // 计算总权重
            double totalWeight = weights.Values.Sum();
            
            // 生成随机数
            double randomValue = _random.NextDouble() * totalWeight;
            
            // 根据权重选择
            double cumulative = 0;
            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (randomValue <= cumulative)
                {
                    return kvp.Key;
                }
            }
            
            // 如果由于浮点精度问题未选择，返回最后一个
            return weights.Keys.Last();
        }

        /// <summary>
        /// 更新概率信息
        /// </summary>
        private void UpdateProbabilities()
        {
            _currentProbabilities.Clear();
            
            if (_candidatePool != null && _candidatePool.Count == 0) return;
            
            var weights = CalculateWeights();
            double totalWeight = weights.Values.Sum();
            
            foreach (var kvp in weights)
            {
                _currentProbabilities[kvp.Key] = kvp.Value / totalWeight;
            }
            
            // 为不在候选池中的成员设置概率为0
            foreach (var number in _allNumbers.Where(n => _candidatePool != null && !_candidatePool.Contains(n)))
            {
                _currentProbabilities[number] = 0;
            }
        }

        #endregion
    }

    /// <summary>
    /// 扩展类：支持按行列抽取（模拟二维数组）
    /// </summary>
    public class BalancedRand2D : BalancedRand
    {
        private int _rows;
        private int _cols;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="minPoolSize">最小候选池大小</param>
        /// <param name="maxGapThreshold">最大抽取次数差距阈值</param>
        /// <param name="coldStartBoost">冷启动提升系数</param>
        /// <param name="decayFactor">权重衰减因子</param>
        public BalancedRand2D(int rows, int cols, int minPoolSize = 3, 
                            int maxGapThreshold = 5, double coldStartBoost = 2.0, 
                            double decayFactor = 0.7) 
            : base(0, rows * cols - 1, minPoolSize, maxGapThreshold, coldStartBoost, decayFactor)
        {
            _rows = rows;
            _cols = cols;
        }
        
        /// <summary>
        /// 抽取一个位置（返回行列）
        /// </summary>
        /// <returns>(行, 列)</returns>
        public (int row, int col) DrawPosition()
        {
            int number = Draw();
            return (number / _cols, number % _cols);
        }
        
        /// <summary>
        /// 批量抽取多个位置
        /// </summary>
        public List<(int row, int col)> DrawMultiplePositions(int count)
        {
            var numbers = DrawMultiple(count);
            return numbers.Select(n => (n / _cols, n % _cols)).ToList();
        }
        
        /// <summary>
        /// 获取位置统计信息
        /// </summary>
        public Dictionary<(int row, int col), int> GetPositionStatistics()
        {
            var stats = GetStatistics();
            return stats.ToDictionary(
                kv => (kv.Key / _cols, kv.Key % _cols),
                kv => kv.Value
            );
        }
    }
}