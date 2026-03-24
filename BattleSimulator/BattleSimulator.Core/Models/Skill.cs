using System; using System.Collections.Generic;

namespace BattleSimulator.Core.Models
{
    /// <summary>
    /// 技能基类，定义了战斗中可以使用的各种技能
    /// </summary>
    public class Skill
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ManaCost { get; set; } = 0;
        public int Cooldown { get; set; } = 0;
        public int CurrentCooldown { get; set; } = 0;
        public SkillTargetType TargetType { get; set; } = SkillTargetType.SingleEnemy;
        public SkillEffectType EffectType { get; set; } = SkillEffectType.Damage;
        
        /// <summary>
        /// 重置技能冷却
        /// </summary>
        public virtual void ResetCooldown()
        {
            CurrentCooldown = 0;
        }
        
        /// <summary>
        /// 更新技能冷却
        /// </summary>
        public virtual void UpdateCooldown()
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown--;
            }
        }
        
        /// <summary>
        /// 检查技能是否可以使用
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <returns>是否可以使用</returns>
        public virtual bool CanUse(Entity caster)
        {
            return caster.IsAlive && CurrentCooldown <= 0;
        }
        
        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="targets">目标列表</param>
        /// <returns>技能使用结果</returns>
        public virtual SkillResult Use(Entity caster, List<Entity> targets)
        {
            // 检查技能是否可以使用
            if (!CanUse(caster))
            {
                return new SkillResult { Success = false, Message = "技能无法使用" };
            }
            
            // 检查目标是否有效
            if (targets == null || targets.Count == 0)
            {
                return new SkillResult { Success = false, Message = "没有有效的目标" };
            }
            
            // 应用技能效果
            var result = ApplyEffect(caster, targets);
            
            // 设置冷却时间
            if (result.Success)
            {
                CurrentCooldown = Cooldown;
            }
            
            return result;
        }
        
        /// <summary>
        /// 应用技能效果
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="targets">目标列表</param>
        /// <returns>技能效果结果</returns>
        protected virtual SkillResult ApplyEffect(Entity caster, List<Entity> targets)
        {
            var result = new SkillResult { Success = true };
            
            switch (EffectType)
            {
                case SkillEffectType.Damage:
                    ApplyDamageEffect(caster, targets, result);
                    break;
                case SkillEffectType.Heal:
                    ApplyHealEffect(caster, targets, result);
                    break;
                case SkillEffectType.Buff:
                    ApplyBuffEffect(caster, targets, result);
                    break;
                case SkillEffectType.Debuff:
                    ApplyDebuffEffect(caster, targets, result);
                    break;
                case SkillEffectType.Combo:
                    ApplyComboEffect(caster, targets, result);
                    break;
            }
            
            return result;
        }
        
        /// <summary>
        /// 应用伤害效果
        /// </summary>
        protected virtual void ApplyDamageEffect(Entity caster, List<Entity> targets, SkillResult result)
        {
            foreach (var target in targets)
            {
                if (!target.IsAlive)
                    continue;
                
                // 计算基础伤害
                int baseDamage = CalculateBaseDamage(caster, target);
                
                // 检查是否命中
                if (IsHit(caster, target))
                {
                    // 检查是否暴击
                    bool isCritical = IsCritical(caster);
                    
                    // 计算最终伤害
                    int finalDamage = CalculateFinalDamage(baseDamage, isCritical, caster);
                    
                    // 目标受到伤害
                    int actualDamage = target.TakeDamage(finalDamage);
                    
                    // 添加伤害记录
                    result.DamageDealt.Add(new DamageRecord { Target = target, Amount = actualDamage, IsCritical = isCritical });
                    
                    // 更新消息
                    if (isCritical)
                    {
                        result.Message += $"{caster.Name} 对 {target.Name} 造成了 {actualDamage} 点暴击伤害！\n";
                    }
                    else
                    {
                        result.Message += $"{caster.Name} 对 {target.Name} 造成了 {actualDamage} 点伤害！\n";
                    }
                }
                else
                {
                    result.Message += $"{caster.Name} 的攻击被 {target.Name} 闪避了！\n";
                }
            }
        }
        
        /// <summary>
        /// 应用治疗效果
        /// </summary>
        protected virtual void ApplyHealEffect(Entity caster, List<Entity> targets, SkillResult result)
        {
            foreach (var target in targets)
            {
                if (!target.IsAlive)
                    continue;
                
                // 计算治疗量
                int healAmount = CalculateHealAmount(caster);
                
                // 目标获得治疗
                int actualHeal = target.Heal(healAmount);
                
                // 添加治疗记录
                result.HealingDone.Add(new HealingRecord { Target = target, Amount = actualHeal });
                
                // 更新消息
                result.Message += $"{caster.Name} 为 {target.Name} 恢复了 {actualHeal} 点生命值！\n";
            }
        }
        
        /// <summary>
        /// 应用增益效果
        /// </summary>
        protected virtual void ApplyBuffEffect(Entity caster, List<Entity> targets, SkillResult result)
        {
            // 子类实现具体的增益效果
        }
        
        /// <summary>
        /// 应用减益效果
        /// </summary>
        protected virtual void ApplyDebuffEffect(Entity caster, List<Entity> targets, SkillResult result)
        {
            // 子类实现具体的减益效果
        }
        
        /// <summary>
        /// 应用组合效果
        /// </summary>
        protected virtual void ApplyComboEffect(Entity caster, List<Entity> targets, SkillResult result)
        {
            // 子类实现具体的组合效果
        }
        
        /// <summary>
        /// 计算基础伤害
        /// </summary>
        protected virtual int CalculateBaseDamage(Entity caster, Entity target)
        {
            // 基础伤害公式：(攻击者攻击力 - 目标防御力/2)，确保至少造成1点伤害
            int damage = caster.Attack - target.Defense / 2;
            return Math.Max(1, damage);
        }
        
        /// <summary>
        /// 计算最终伤害
        /// </summary>
        protected virtual int CalculateFinalDamage(int baseDamage, bool isCritical, Entity caster)
        {
            if (isCritical)
            {
                // 暴击伤害 = 基础伤害 * (1 + 暴击伤害加成/100)
                return (int)(baseDamage * (1 + caster.CritDamage / 100.0));
            }
            return baseDamage;
        }
        
        /// <summary>
        /// 计算治疗量
        /// </summary>
        protected virtual int CalculateHealAmount(Entity caster)
        {
            // 基础治疗量 = 施法者攻击力 * 0.8（示例值）
            return (int)(caster.Attack * 0.8);
        }
        
        /// <summary>
        /// 检查是否命中
        /// </summary>
        protected virtual bool IsHit(Entity caster, Entity target)
        {
            // 命中率 = 攻击者命中率 - 目标闪避率
            int hitChance = caster.HitRate - target.DodgeRate;
            // 确保命中率在5%到95%之间
            hitChance = Math.Max(5, Math.Min(95, hitChance));
            
            // 随机数生成（实际项目中应该使用更好的随机数生成器）
            return new Random().Next(100) < hitChance;
        }
        
        /// <summary>
        /// 检查是否暴击
        /// </summary>
        protected virtual bool IsCritical(Entity caster)
        {
            // 暴击率受施法者暴击属性影响
            int critChance = caster.CritRate;
            // 确保暴击率在1%到50%之间
            critChance = Math.Max(1, Math.Min(50, critChance));
            
            // 随机数生成
            return new Random().Next(100) < critChance;
        }
    }
    
    /// <summary>
    /// 技能目标类型
    /// </summary>
    public enum SkillTargetType
    {
        SingleEnemy,
        SingleAlly,
        AllEnemies,
        AllAllies,
        Self,
        All
    }
    
    /// <summary>
    /// 技能效果类型
    /// </summary>
    public enum SkillEffectType
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        Combo
    }
    
    /// <summary>
    /// 技能使用结果
    /// </summary>
    public class SkillResult
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public List<DamageRecord> DamageDealt { get; } = new List<DamageRecord>();
        public List<HealingRecord> HealingDone { get; } = new List<HealingRecord>();
        public List<StatusEffect> StatusEffectsApplied { get; } = new List<StatusEffect>();
    }
    
    /// <summary>
    /// 伤害记录
    /// </summary>
    public class DamageRecord
    {
        public Entity Target { get; set; } = null!;
        public int Amount { get; set; } = 0;
        public bool IsCritical { get; set; } = false;
    }
    
    /// <summary>
    /// 治疗记录
    /// </summary>
    public class HealingRecord
    {
        public Entity Target { get; set; } = null!;
        public int Amount { get; set; } = 0;
    }
}