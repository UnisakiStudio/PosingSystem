"""Reject directory redirection in disposable Unity caches without following any links."""
import os
import pathlib
import stat


def cloud_tag(tag):
    # Microsoft MS-FSCC Reparse Tags: CLOUD and CLOUD_1 through CLOUD_F.
    return tag & 0xFFFF0FFF == 0x9000001A


def scan(root):
    # lexists also sees a dangling junction/symlink; exists would silently skip it.
    pending = [str(root)] if os.path.lexists(root) else []
    checked = 0
    while pending:
        directory = pending.pop()
        root_info = os.lstat(directory)
        if root_info.st_file_attributes & stat.FILE_ATTRIBUTE_REPARSE_POINT and not cloud_tag(root_info.st_reparse_tag):
            raise RuntimeError(f'Unsafe Unity cache root: {directory}, tag={root_info.st_reparse_tag:#x}')
        with os.scandir(directory) as entries:
            for entry in entries:
                info = entry.stat(follow_symlinks=False)
                if info.st_file_attributes & stat.FILE_ATTRIBUTE_REPARSE_POINT:
                    if not cloud_tag(info.st_reparse_tag):
                        raise RuntimeError(f'Unsafe Unity cache link: {entry.path}, tag={info.st_reparse_tag:#x}')
                if stat.S_ISDIR(info.st_mode): pending.append(entry.path)
                checked += 1
    return checked


if __name__ == '__main__':
    project = pathlib.Path(__file__).resolve().parents[2]
    checked = sum(scan(project / cache) for cache in ('Temp', 'Library'))
    print(f'Unity cache link check passed: {checked} entries')
