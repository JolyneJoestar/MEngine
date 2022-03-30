using System.Collections.Generic;


public class LRUCache 
{
    private Dictionary<int, LinkedListNode<int>> m_map = new Dictionary<int, LinkedListNode<int>>();
    private LinkedList<int> m_list = new LinkedList<int>();

    public int First { get { return m_list.First.Value; } }

    public void Add(int id)
    {
        if (m_map.ContainsKey(id))
            return;
        var node = new LinkedListNode<int>(id);
        m_map.Add(id, node);
        m_list.AddLast(node);
    }

    public bool SetActive(int id)
    {
        LinkedListNode<int> node = null;
        if (!m_map.TryGetValue(id, out node))
            return false;
        m_list.Remove(node);
        m_list.AddLast(node);

        return true;
    }
}
