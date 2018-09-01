CREATE TABLE rating
(
    character_id integer NOT NULL,
    rating real,
    rd real,
    updated_at timestamp with time zone,
    PRIMARY KEY (character_id),
    CONSTRAINT character_id FOREIGN KEY (character_id)
        REFERENCES character(id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)