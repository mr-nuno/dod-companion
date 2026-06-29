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
    <div className="space-y-4">
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
          className="rounded-lg bg-runecyan-600 px-4 py-2 text-sm font-semibold text-charcoal-950 transition hover:bg-runecyan-500 disabled:opacity-50"
        >
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      {error && <p className="text-sm text-dodred-600 dark:text-dodred-300">{error}</p>}

      {result && (
        <div className="space-y-3">
          <p className="text-xs text-charcoal-400 dark:text-bonewhite-300/60">
            {result.totalHits} result{result.totalHits === 1 ? '' : 's'} for “{result.query}”
          </p>
          {result.results.map((hit, index) => (
            <article
              key={`${hit.sourceFileName}-${hit.physicalPageNumber}-${index}`}
              className="rounded-lg border border-bonewhite-200 bg-white p-4 dark:border-charcoal-700 dark:bg-charcoal-900/60"
            >
              <header className="mb-2 flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between sm:gap-2">
                <h3 className="font-semibold text-runecyan-600 dark:text-runecyan-400">
                  {hit.header ?? 'Result'}
                </h3>
                <span className="text-xs text-charcoal-400 dark:text-bonewhite-300/60">
                  {hit.sourceFileName} · p.{hit.physicalPageNumber}
                </span>
              </header>
              <div className="prose prose-sm max-w-none dark:prose-invert">
                <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>
                  {hit.content}
                </Markdown>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  );
};
