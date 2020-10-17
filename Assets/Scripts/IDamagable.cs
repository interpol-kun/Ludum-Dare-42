public interface IDamagable {
    //Дамагер - наносящий урон. Нужно будет для зачисления очков игроку, или усиления мобов за попадание по игроку
    void TakeDamage(float damageToTake, IDamagable damager);
    void Death();
}
