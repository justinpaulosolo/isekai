CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    display_name TEXT,
    email TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE user_logins (
    provider TEXT NOT NULL, -- 'google', 'github', 'discord'
    provider_subject TEXT NOT NULL, -- the provider's stable user id
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (provider, provider_subject)
);

CREATE INDEX idx_user_logins_user_id ON user_logins(user_id);

ALTER TABLE short_urls
    ADD COLUMN user_id BIGINT NULL REFERENCES users(id) ON DELETE SET NULL;
CREATE INDEX idx_short_urls_user_id on short_urls(user_id) WHERE user_id IS NOT NULL;