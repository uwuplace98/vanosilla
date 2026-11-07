-- ============================================
-- Vanosilla BazaarServer Datenbank-Schema
-- Wirtschaftssystem & Trading Management
-- ============================================

-- Create Database (durch SQL Client oder psql)
-- CREATE DATABASE bazaar;

-- --------------------------------------------
-- Tab: Accounts mit Wallet-Data
-- --------------------------------------------

CREATE TABLE bazaar.accounts (
    id SERIAL PRIMARY KEY,
    account_id VARCHAR(50) NOT NULL UNIQUE,
    character_name VARCHAR(100) NOT NULL,
    server_id INTEGER,  -- Server ID für Distribution

    -- Wallet-Balance & Trading Data
    balance DECIMAL(18,2) DEFAULT 0,
    gold_total DECIMAL(18,2) DEFAULT 0,
    silver_total DECIMAL(18,2) DEFAULT 0,

    -- Trading Limits
    daily_trade_limit INT DEFAULT 1000000,
    week_trade_limit INT DEFAULT 5000000,

    -- Status Flags
    is_active BOOLEAN DEFAULT TRUE,
    last_login TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP

);

-- Index für schnelle Account-Looks up
CREATE INDEX idx_bazaar_accounts_account_id ON bazaar.accounts(account_id);
CREATE INDEX idx_bazaar_accounts_server_id ON bazaar.accounts(server_id);

-- --------------------------------------------
-- Tab: Transactions (Herkunft/Abfluss)
-- --------------------------------------------

CREATE TABLE bazaar.transactions (
    id SERIAL PRIMARY KEY,
    transaction_id VARCHAR(100) NOT NULL UNIQUE,
    account_id INTEGER NOT NULL,

    -- Trading Data
    type VARCHAR(50) NOT NULL,  -- buy, sell, refund, transfer
    item_id INT,                 -- Item ID für Items/Trades
    quantity INT DEFAULT 1,

    -- Price Data
    price DECIMAL(18,2),         -- Buy/Sell-Preis pro Stück

    -- Server Info (gRPC)
    source_server_id INTEGER,
    target_server_id INTEGER,

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_transactions_account FOREIGN KEY (account_id)
        REFERENCES bazaar.accounts(id) ON DELETE CASCADE

);

-- Index für schnelle Transaktions-Looks up
CREATE INDEX idx_bazaar_transactions_account ON bazaar.transactions(account_id);
CREATE INDEX idx_bazaar_transactions_type ON bazaar.transactions(type);

-- --------------------------------------------
-- Tab: Items & Inventory Management
-- --------------------------------------------

CREATE TABLE bazaar.inventory_items (
    id SERIAL PRIMARY KEY,
    item_id INT NOT NULL UNIQUE,  -- Globale Item ID aus .dat Dateien

    -- Basic Item Properties
    name VARCHAR(200) NOT NULL,
    description TEXT,

    -- Trading Value
    base_price DECIMAL(18,2),
    buy_price DECIMAL(18,2),
    sell_price DECIMAL(18,2),

    -- Rarity/Quality Tags
    rarity VARCHAR(50) DEFAULT 'common',
    quality INT DEFAULT 1,

    -- Server-specific overrides
    server_min_price DECIMAL(18,2),
    server_max_price DECIMAL(18,2),

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP

);

-- Index für schnelle Item-Looks up
CREATE INDEX idx_bazaar_items_item_id ON bazaar.inventory_items(item_id);

-- --------------------------------------------
-- Tab: User Preferences & Features
-- --------------------------------------------

CREATE TABLE bazaar.preferences (
    id SERIAL PRIMARY KEY,
    account_id INTEGER NOT NULL,

    -- Trading Settings
    auto_sell BOOLEAN DEFAULT FALSE,
    auto_buy BOOLEAN DEFAULT FALSE,
    max_traded_items INT DEFAULT 100,

    -- Notification Settings
    enable_mail_notifications BOOLEAN DEFAULT TRUE,

    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_preferences_account FOREIGN KEY (account_id)
        REFERENCES bazaar.accounts(id) ON DELETE CASCADE

);

CREATE INDEX idx_bazaar_preferences_account ON bazaar.preferences(account_id);

-- --------------------------------------------
-- View: Trading Summary (für Dashboard/API)
-- --------------------------------------------

CREATE VIEW v_bazaar_accounts_summary AS
SELECT
    a.id,
    a.account_id,
    a.character_name,
    a.balance,
    a.gold_total,
    a.silver_total,
    COALESCE(SUM(t.price * t.quantity), 0) AS total_value_from_transactions,
    a.last_login,
    a.created_at
FROM bazaar.accounts a
LEFT JOIN bazaar.transactions t ON a.id = t.account_id
GROUP BY a.id;

-- --------------------------------------------
-- View: Daily/Monthly Activity (Analytics)
-- --------------------------------------------

CREATE VIEW v_bazaar_daily_activity AS
SELECT
    DATE(created_at) AS activity_date,
    type,
    COUNT(*) AS transaction_count,
    COALESCE(SUM(ABS(price * quantity)), 0) AS total_volume
FROM bazaar.transactions
GROUP BY DATE(created_at), type;

-- ============================================
-- Init: Sample Data für Testing
-- ============================================

-- Beispiel-User erstellen (für API/Integrationstests)
INSERT INTO bazaar.accounts
    (account_id, character_name, balance, is_active)
VALUES
    ('ACC_TEST_001', 'TestCharacterA', 5000000, TRUE),
    ('ACC_TEST_002', 'TestCharacterB', 10000000, TRUE);

-- Beispiel-Item erstellen (für Trading Tests)
INSERT INTO bazaar.inventory_items
    (item_id, name, base_price, rarity, server_min_price, server_max_price)
VALUES
    (1001, 'Common Weapon', 5000, 'common', 4500, 6000),
    (1002, 'Rare Sword', 25000, 'rare', 20000, 35000);

-- ============================================
-- Index: Performance Optimisation
-- ============================================

CREATE INDEX idx_bazaar_transactions_created ON bazaar.transactions(created_at DESC);
