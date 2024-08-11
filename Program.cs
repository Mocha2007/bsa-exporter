static class Program {
	static int Main(string[] args){
		string path = args[0];
		byte[] rawData = File.ReadAllBytes(path); // .Skip(24)
		uint magic = BitConverter.ToUInt32(rawData.Take(4).ToArray());
		if (magic != 0x00000100){
			Console.WriteLine("magic number is bad magic :<");
			return 1;
		}
		// Offset of the hash table in the file, minus the header size (12).
		uint hash_table_offset = BitConverter.ToUInt32(rawData.Skip(4).Take(4).ToArray());
		uint file_count = BitConverter.ToUInt32(rawData.Skip(8).Take(4).ToArray());
		int offset = 12;
		// file sizes and offsets
		uint[,] file_sizes_and_offsets = new uint[file_count, 2];
		for (int i = 0; i < file_count; i++){
			// size
			file_sizes_and_offsets[i, 0] = BitConverter.ToUInt32(rawData.Skip(offset).Take(4).ToArray());
			offset += 4;
			// offset
			file_sizes_and_offsets[i, 1] = BitConverter.ToUInt32(rawData.Skip(offset).Take(4).ToArray());
			offset += 4;
		}
		// name offsets
		uint[] name_offsets = new uint[file_count];
		for (int i = 0; i < file_count; i++){
			name_offsets[i] = BitConverter.ToUInt32(rawData.Skip(offset).Take(4).ToArray());
			offset += 4;
		}
		// strings
		int name_section_offset = offset;
		string[] names = new string[file_count];
		for (int i = 0; i < file_count; i++)
			names[i] = System.Text.Encoding.ASCII.GetString(rawData.Skip(name_section_offset + (int)name_offsets[i]).Take((int)(name_offsets[i+1] - name_offsets[i])).ToArray());
		// hashes
		ulong[] hashes = new ulong[file_count];
		for (int i = 0; i < file_count; i++)
			hashes[i] = BitConverter.ToUInt64(rawData.Skip(12 + (int)hash_table_offset + 8*i).Take(8).ToArray());
		// data
		byte[][] data = new byte[file_count][];
		for (int i = 0; i < file_count; i++)
			data[i] = rawData.Skip(12 + (int)file_sizes_and_offsets[i, 1]).Take((int)file_sizes_and_offsets[i, 0]).ToArray();
		// save
		for (int i = 0; i < names.Length; i++)
			File.WriteAllBytes(names[i], data[i]);
		// its oki
		return 0;
	}
}