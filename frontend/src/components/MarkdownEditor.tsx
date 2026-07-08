import { useEditor, EditorContent, Extension, type Editor } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import { Markdown } from 'tiptap-markdown';

// Make Enter insert a single line break instead of splitting into a new paragraph.
// Lists and code blocks keep their default Enter behaviour (new item / newline).
const EnterAsHardBreak = Extension.create({
  name: 'enterAsHardBreak',
  priority: 1000,
  addKeyboardShortcuts() {
    return {
      Enter: () => {
        if (this.editor.isActive('codeBlock') || this.editor.isActive('listItem')) {
          return false;
        }
        return this.editor.commands.setHardBreak();
      },
    };
  },
});

interface MarkdownEditorProps {
  /** Initial markdown content. Change the component `key` to re-initialise (e.g. reset after submit). */
  value?: string;
  onChange: (markdown: string) => void;
}

// tiptap-markdown augments the editor storage at runtime but ships no v3 types for it.
const getMarkdown = (editor: Editor): string =>
  (editor.storage as unknown as { markdown: { getMarkdown: () => string } }).markdown.getMarkdown();

const btnBase =
  'rounded px-2 py-1 text-xs font-semibold transition select-none';
const btnIdle =
  'text-charcoal-500 hover:bg-bonewhite-200 dark:text-bonewhite-400 dark:hover:bg-charcoal-700';
const btnActive = 'bg-dragongreen-600 text-white';

interface ToolButtonProps {
  editor: Editor;
  label: string;
  title: string;
  isActive: boolean;
  onClick: () => void;
}

const ToolButton = ({ label, title, isActive, onClick }: ToolButtonProps) => (
  <button
    type="button"
    title={title}
    onMouseDown={(e) => e.preventDefault()}
    onClick={onClick}
    className={`${btnBase} ${isActive ? btnActive : btnIdle}`}
  >
    {label}
  </button>
);

export const MarkdownEditor = ({ value = '', onChange }: MarkdownEditorProps) => {
  const editor = useEditor({
    extensions: [StarterKit, Markdown, EnterAsHardBreak],
    content: value,
    onUpdate: ({ editor }) => onChange(getMarkdown(editor)),
    editorProps: {
      attributes: {
        class:
          'prose prose-sm min-h-[8rem] max-w-none px-3 py-2 focus:outline-none dark:prose-invert',
      },
    },
  });

  if (!editor) {
    return null;
  }

  return (
    <div className="overflow-hidden rounded-lg border border-bonewhite-200 dark:border-charcoal-600">
      <div className="flex flex-wrap gap-0.5 border-b border-bonewhite-200 bg-bonewhite-100 px-1.5 py-1 dark:border-charcoal-600 dark:bg-charcoal-800">
        <ToolButton editor={editor} label="B" title="Bold" isActive={editor.isActive('bold')} onClick={() => editor.chain().focus().toggleBold().run()} />
        <ToolButton editor={editor} label="I" title="Italic" isActive={editor.isActive('italic')} onClick={() => editor.chain().focus().toggleItalic().run()} />
        <ToolButton editor={editor} label="H2" title="Heading" isActive={editor.isActive('heading', { level: 2 })} onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()} />
        <ToolButton editor={editor} label="H3" title="Subheading" isActive={editor.isActive('heading', { level: 3 })} onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()} />
        <ToolButton editor={editor} label="• List" title="Bullet list" isActive={editor.isActive('bulletList')} onClick={() => editor.chain().focus().toggleBulletList().run()} />
        <ToolButton editor={editor} label="1. List" title="Numbered list" isActive={editor.isActive('orderedList')} onClick={() => editor.chain().focus().toggleOrderedList().run()} />
        <ToolButton editor={editor} label="❝" title="Quote" isActive={editor.isActive('blockquote')} onClick={() => editor.chain().focus().toggleBlockquote().run()} />
        <ToolButton editor={editor} label="‹›" title="Inline code" isActive={editor.isActive('code')} onClick={() => editor.chain().focus().toggleCode().run()} />
        <ToolButton editor={editor} label="{ }" title="Code block" isActive={editor.isActive('codeBlock')} onClick={() => editor.chain().focus().toggleCodeBlock().run()} />
      </div>
      <EditorContent editor={editor} />
    </div>
  );
};
