using System;
using System.Collections.Generic;

namespace BankSystem
{
    public class BankAccount
    {
        private decimal _balance;
        private readonly List<string> _transactions = new();
        
        
        public BankAccount(string ownerName, decimal initialBalance = 0)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
                throw new ArgumentException("Owner name cannot be empty");
            
            OwnerName = ownerName;
            _balance = initialBalance;
            _transactions.Add($"Account created with balance: {initialBalance:C}");
        }
        
        public string OwnerName { get; }
        
        public decimal Balance => _balance;
        
        public IReadOnlyList<string> Transactions => _transactions.AsReadOnly();
        
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");
            
            _balance += amount;
            _transactions.Add($"Deposited: {amount:C}, New balance: {_balance:C}");
        }
        
        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");
            
            if (amount > _balance)
                return false;
            
            _balance -= amount;
            _transactions.Add($"Withdrew: {amount:C}, New balance: {_balance:C}");
            return true;
        }
        
        public string GetAccountSummary()
        {
            return $"Account owner: {OwnerName}, Current balance: {_balance:C}, Total transactions: {_transactions.Count}";
        }
        
        public void TransferTo(BankAccount target, decimal amount)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            
            if (amount <= 0)
                throw new ArgumentException("Transfer amount must be positive");
            
            if (Withdraw(amount))
            {
                target.Deposit(amount);
                _transactions.Add($"Transferred: {amount:C} to {target.OwnerName}");
            }
            else
            {
                throw new InvalidOperationException("Insufficient funds for transfer");
            }
        }
        
        public string FormatBalance(string format)
        {
            return _balance.ToString(format);
        }
        
        public string FormatBalance()
        {
            return _balance.ToString("C");
        }
        
        public void ClearTransactionHistory()
        {
            _transactions.Clear();
            _transactions.Add("Transaction history cleared");
        }
    }
    
    public class InterestCalculator
    {
        public decimal CalculateSimpleInterest(decimal principal, decimal rate, int years)
        {
            return principal * (rate / 100) * years;
        }
        
        public decimal CalculateCompoundInterest(decimal principal, decimal rate, int years)
        {
            return principal * (decimal)Math.Pow((double)(1 + rate / 100), years) - principal;
        }
        
        public void ValidateInput(decimal principal, decimal rate)
        {
            if (principal <= 0)
                throw new ArgumentException("Principal must be positive");
            if (rate <= 0)
                throw new ArgumentException("Rate must be positive");
        }
    }
}