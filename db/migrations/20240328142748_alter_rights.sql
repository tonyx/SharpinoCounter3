-- migrate:up
GRANT ALL ON TABLE public.events_01_countercontext TO safe;
GRANT ALL ON TABLE public.snapshots_01_countercontext TO safe;
GRANT ALL ON SEQUENCE public.snapshots_01_countercontext_id_seq TO safe;
GRANT ALL ON SEQUENCE public.events_01_countercontext_id_seq TO safe;


-- migrate:down

