CREATE TABLE short_urls (
	id BIGSERIAL PRIMARY KEY,
	code VARCHAR(10) UNIQUE  NOT NULL,
	long_url TEXT NOT NULL,
	created_at TIMESTAMP NOT NULL DEFAULT now(),
	click_count BIGINT NOT NULL DEFAULT 0
);
CREATE INDEX idx_short_urls_code on short_urls(code);