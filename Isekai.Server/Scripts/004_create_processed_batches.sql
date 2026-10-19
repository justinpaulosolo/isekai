CREATE TABLE processed_batches (
    batch_id     uuid        PRIMARY KEY,
    processed_at timestamptz NOT NULL DEFAULT now()
);

-- Lets you purge old rows cheaply later (dedupe only needs to cover the redelivery window).
CREATE INDEX ix_processed_batches_processed_at ON processed_batches (processed_at);