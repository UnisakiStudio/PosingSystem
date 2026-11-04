import pathlib
import stat
import types
import unittest
from unittest.mock import Mock, patch
import check_unity_cache_links as guard


class CacheLinkGuardTests(unittest.TestCase):
    def test_all_cloud_tags_are_allowed_and_redirection_tags_rejected(self):
        for i in range(16): self.assertTrue(guard.cloud_tag(0x9000001A | (i << 12)))
        for tag in [0, 0xA0000003, 0xA000000C, 0x8000001B, 0x9000001C, 0xA000001A]:
            self.assertFalse(guard.cloud_tag(tag), hex(tag))

    def test_cache_root_link_is_rejected_before_enumeration(self):
        root = pathlib.Path('cache')
        info = types.SimpleNamespace(st_file_attributes=stat.FILE_ATTRIBUTE_REPARSE_POINT, st_reparse_tag=0xA0000003)
        with patch.object(guard.os.path, 'lexists', return_value=True), patch.object(guard.os, 'lstat', return_value=info), patch.object(guard.os, 'scandir') as scan:
            with self.assertRaises(RuntimeError): guard.scan(root)
            scan.assert_not_called()

    def test_nested_directory_link_is_rejected_without_following_it(self):
        root = pathlib.Path('cache')
        regular = types.SimpleNamespace(st_file_attributes=0, st_reparse_tag=0, st_mode=stat.S_IFDIR)
        link = types.SimpleNamespace(st_file_attributes=stat.FILE_ATTRIBUTE_REPARSE_POINT, st_reparse_tag=0xA0000003, st_mode=stat.S_IFDIR)
        entry = Mock(path='cache/saved-source-link'); entry.stat.return_value = link
        with patch.object(guard.os.path, 'lexists', return_value=True), patch.object(guard.os, 'lstat', return_value=regular), patch.object(guard.os, 'scandir') as scan:
            scan.return_value.__enter__.return_value = [entry]
            with self.assertRaises(RuntimeError): guard.scan(root)
            scan.assert_called_once()
            entry.stat.assert_called_once_with(follow_symlinks=False)


if __name__ == '__main__': unittest.main()
