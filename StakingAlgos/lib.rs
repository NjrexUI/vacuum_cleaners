use anchor_lang::prelude::*;
use anchor_spl::token::{self, TokenAccount, Token, Transfer};

declare_id!("---");

#[program]
pub mod solana_staking {
    use super::*;

    pub fn initialize_pool(ctx: Context<InitializePool>, bump: u8) -> Result<()> {
        let pool = &mut ctx.accounts.pool;
        pool.authority = *ctx.accounts.authority.key;
        pool.bump = bump;
        pool.mint = ctx.accounts.mint.key();
        pool.pool_account = ctx.accounts.pool_token_account.key();
        pool.total_staked = 0;
        Ok(())
    }

    pub fn stake(ctx: Context<Stake>, amount: u64) -> Result<()> {
        let pool = &mut ctx.accounts.pool;
        let stake_acc = &mut ctx.accounts.stake_account;

        let cpi_accounts = Transfer {
            from: ctx.accounts.user_token_account.to_account_info(),
            to: ctx.accounts.pool_token_account.to_account_info(),
            authority: ctx.accounts.user.to_account_info(),
        };
        let cpi_program = ctx.accounts.token_program.to_account_info();
        token::transfer(CpiContext::new(cpi_program, cpi_accounts), amount)?;

        stake_acc.owner = ctx.accounts.user.key();
        stake_acc.amount = stake_acc.amount.checked_add(amount).unwrap();
        stake_acc.last_update = Clock::get()?.unix_timestamp;
        pool.total_staked = pool.total_staked.checked_add(amount).unwrap();

        Ok(())
    }

    pub fn unstake(ctx: Context<Unstake>, amount: u64) -> Result<()> {
        let pool = &mut ctx.accounts.pool;
        let stake_acc = &mut ctx.accounts.stake_account;
        require!(stake_acc.amount >= amount, ErrorCode::InsufficientStake);

        stake_acc.amount = stake_acc.amount.checked_sub(amount).unwrap();
        pool.total_staked = pool.total_staked.checked_sub(amount).unwrap();

        let seeds = &[b"pool".as_ref(), pool.authority.as_ref(), &[pool.bump]];
        let signer = &[&seeds[..]];
        let cpi_accounts = Transfer {
            from: ctx.accounts.pool_token_account.to_account_info(),
            to: ctx.accounts.user_token_account.to_account_info(),
            authority: ctx.accounts.pool_signer.to_account_info(),
        };
        token::transfer(CpiContext::new_with_signer(ctx.accounts.token_program.to_account_info(), cpi_accounts, signer), amount)?;
        Ok(())
    }

    pub fn deposit_rewards(ctx: Context<DepositRewards>, amount: u64) -> Result<()> {
        ctx.accounts.pool.reward_balance = ctx.accounts.pool.reward_balance.checked_add(amount).unwrap();
        Ok(())
    }
}