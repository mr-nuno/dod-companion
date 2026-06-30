import { useState } from 'react';
import Markdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeSanitize from 'rehype-sanitize';
import { useRuleSearch } from '@hooks/useRuleSearch';

export const RuleSearchPanel = () => {
  const { search, result, loading, error } = useRuleSearch();
  const [query, setQuery] = useState('');

  const onSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    void search(query);
  };

  return (
    <div className="flex flex-col h-full space-y-6">
      <form onSubmit={onSubmit} className="flex gap-2">
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Ask a rules question…"
          className="flex-1 rounded-lg border border-bonewhite-200 bg-bonewhite-50 px-3 py-2 text-sm text-charcoal-900 outline-none focus:border-runecyan-500 dark:border-charcoal-600 dark:bg-charcoal-950 dark:text-bonewhite-500"
        />
        <button
          type="submit"
          disabled={loading}
          className="rounded-lg bg-runecyan-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-runecyan-500 disabled:opacity-50"
        >
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      {error && (
        <div className="rounded-xl border border-dodred-500/20 bg-dodred-500/10 p-4 text-sm font-medium text-dodred-600 backdrop-blur-sm dark:text-dodred-400">
          {error}
        </div>
      )}

      {result && (
        <div className="flex flex-col gap-4 overflow-y-auto pr-2 pb-4 scrollbar-thin scrollbar-track-transparent scrollbar-thumb-charcoal-200 dark:scrollbar-thumb-charcoal-700">
          <div className="flex items-end justify-between px-1 pb-2 border-b border-bonewhite-200 dark:border-charcoal-700/50">
            <h3 className="text-sm font-semibold tracking-wide text-charcoal-600 dark:text-bonewhite-400">
              Search Results
            </h3>
            <div className="text-right">
              <p className="text-xs font-medium text-charcoal-500 dark:text-bonewhite-500">
                Found {result.totalHits} match{result.totalHits === 1 ? '' : 'es'}
              </p>
              {result.processedQuery && result.processedQuery !== result.query && (
                <p className="text-[10px] italic text-charcoal-400 dark:text-bonewhite-500/60 mt-0.5">
                  Interpreted as: "{result.processedQuery}"
                </p>
              )}
            </div>
          </div>
          
          <div className="flex flex-col gap-4">
            {result.results.map((hit, index) => (
              <article
                key={`${hit.sourceFileName}-${hit.physicalPageNumber}-${index}`}
                className="group relative flex flex-col gap-3 rounded-2xl border border-white/40 bg-white/40 p-5 shadow-sm backdrop-blur-md transition-all duration-300 hover:-translate-y-1 hover:shadow-xl hover:shadow-runecyan-900/5 dark:border-charcoal-700/40 dark:bg-charcoal-800/40 dark:hover:border-runecyan-500/30 dark:hover:shadow-runecyan-500/10"
              >
                <div className="absolute left-0 top-4 bottom-4 w-1 rounded-r-md bg-gradient-to-b from-runecyan-400 to-dragongreen-400 opacity-0 transition-opacity duration-300 group-hover:opacity-100"></div>
                
                <header className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                  <h4 className="text-lg font-bold bg-clip-text text-transparent bg-gradient-to-r from-runecyan-700 to-dragongreen-600 dark:from-runecyan-400 dark:to-dragongreen-400">
                    {hit.header ?? 'Result'}
                  </h4>
                  <div className="flex items-center gap-2 rounded-lg bg-bonewhite-100/50 px-2.5 py-1 text-[11px] font-semibold tracking-wider text-charcoal-500 shadow-inner dark:bg-charcoal-900/50 dark:text-bonewhite-400">
                    <span className="uppercase">{hit.sourceFileName}</span>
                    <span className="h-1 w-1 rounded-full bg-charcoal-300 dark:bg-charcoal-600"></span>
                    <span>P. {hit.physicalPageNumber + hit.pageModifier}</span>
                  </div>
                </header>

                <div className="prose prose-sm prose-charcoal max-w-none prose-p:leading-relaxed prose-a:text-runecyan-600 hover:prose-a:text-runecyan-500 dark:prose-invert dark:prose-a:text-runecyan-400">
                  <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>
                    {hit.content}
                  </Markdown>
                </div>

                <footer className="mt-2 flex items-center justify-between border-t border-bonewhite-200/50 pt-3 dark:border-charcoal-700/50">
                  <div className="flex flex-wrap gap-1.5">
                    {hit.tags.map((tag) => (
                      <span
                        key={tag}
                        className="rounded-md bg-runecyan-50 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-runecyan-700 ring-1 ring-inset ring-runecyan-600/10 dark:bg-runecyan-500/10 dark:text-runecyan-300 dark:ring-runecyan-400/20"
                      >
                        {tag}
                      </span>
                    ))}
                  </div>
                  <div className="flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-widest text-charcoal-400 dark:text-charcoal-500">
                    <span>Match</span>
                    <span className="rounded bg-charcoal-100 px-1.5 py-0.5 text-charcoal-600 dark:bg-charcoal-800 dark:text-bonewhite-300">
                      {Math.round(hit.searchScore * 100)}%
                    </span>
                  </div>
                </footer>
              </article>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
